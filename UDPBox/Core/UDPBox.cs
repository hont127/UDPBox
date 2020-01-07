using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

//Socket error code: https://docs.microsoft.com/en-us/windows/win32/winsock/windows-sockets-error-codes-2?redirectedfrom=MSDN

namespace Hont.UDPBoxPackage
{
    public class UDPBox : IDisposable
    {
        public struct QueueInfo
        {
            public byte[] Content { get; set; }
            public string IPEndPointAddress_Str { get; set; }
            public int IPEndPoint_Port { get; set; }
        }

        public struct MessageInterceptInfo
        {
            public byte[] Bytes { get; set; }
            public IPEndPoint IPEndPoint { get; set; }
        }

        List<UDPBox_UDPClient> mUdpClientsList;
        byte[] mPackageHeadBytes;
        List<HandlerBase> mHandlerList;
        List<Action> mWorkThreadOperateList;

        Thread mWorkThread;
        Thread mSendMessageThread;

        bool mIsReleased;

        Random mRandom;

        IPEndPoint mCacheIPEndPoint;

        Queue<QueueInfo> mSendQueue;
        Queue<QueueInfo> mRecvQueue;

        ACKRequestProcessor mACKRequestProcessor;

        List<Func<MessageInterceptInfo, bool>> mMessageInterceptList;

        public int SendMsgThreadSleepTime { get; set; } = 7;

        public uint StatisticsBadPackageCount { get; private set; }
        public uint StatisticsTotalPackageCount { get; private set; }

        public IUDPBoxLogger Logger { get; private set; }

        public byte[] PackageHeadBytes { get { return mPackageHeadBytes; } }
        public event Action<byte[], IPEndPoint> OnSendMessage;
        public event Action<Exception> OnException;


        public UDPBox()
        {
            mACKRequestProcessor = new ACKRequestProcessor(this);

            mMessageInterceptList = new List<Func<MessageInterceptInfo, bool>>(4);

            mRandom = new Random();

            mSendQueue = new Queue<QueueInfo>(32);
            mRecvQueue = new Queue<QueueInfo>(32);

            mHandlerList = new List<HandlerBase>(16);
            mWorkThreadOperateList = new List<Action>(16);

            Logger = new EmptyUDPBoxLogger();

            mSendMessageThread = new Thread(SendMessageThreadLoop);
            mSendMessageThread.Priority = ThreadPriority.AboveNormal;
            mWorkThread = new Thread(WorkThreadLoop);
        }

        public UDPBox(UDPBox_UDPClient[] udpClientsArray, string packageHead)
            : this()
        {
            Initialization(udpClientsArray, packageHead);
        }

        public void Initialization(UDPBox_UDPClient[] udpClientsArray, string packageHead)
        {
            mUdpClientsList = new List<UDPBox_UDPClient>(udpClientsArray);
            mPackageHeadBytes = UDPBoxUtility.ToBuffer(packageHead);

            RegistHandler(new PingPongHandler(mPackageHeadBytes));
            RegistHandler(new LargePackageHandler(mPackageHeadBytes));
        }

        public void Start()
        {
            mACKRequestProcessor.Initialization();

            for (int i = 0, iMax = mUdpClientsList.Count; i < iMax; i++)
            {
                var udpClient = mUdpClientsList[i];
                udpClient.BeginReceive(ReceiveMessageCallback, udpClient);
            }

            mIsReleased = false;
            if (mSendMessageThread.ThreadState != ThreadState.Stopped)
                mSendMessageThread = new Thread(SendMessageThreadLoop);
            mSendMessageThread.Start();

            if (mWorkThread.ThreadState != ThreadState.Stopped)
                mWorkThread = new Thread(WorkThreadLoop);
            mWorkThread.Start();
        }

        public void SetLogger(IUDPBoxLogger logger)
        {
            Logger = logger;
        }

        public void RegistMessageIntercept(Func<MessageInterceptInfo, bool> content)
        {
            Logger.Log("RegistMessageIntercept: " + content, EUDPBoxLogType.Log);

            mMessageInterceptList.Add(content);
        }

        public void UnregistMessageIntercept(Func<MessageInterceptInfo, bool> content)
        {
            Logger.Log("UnregistMessageIntercept: " + content, EUDPBoxLogType.Log);

            mMessageInterceptList.Remove(content);
        }

        public void RegistWorkThreadOperate(Action operateAction)
        {
            Logger.Log("RegistWorkThreadOperate: " + operateAction, EUDPBoxLogType.Log);

            mWorkThreadOperateList.Add(operateAction);
        }

        public void UnregistWorkThreadOperate(Action operateAction)
        {
            Logger.Log("UnregistWorkThreadOperate: " + operateAction, EUDPBoxLogType.Log);

            mWorkThreadOperateList.Remove(operateAction);
        }

        public void RegistHandler(HandlerBase handler)
        {
            Logger.Log("RegistHandler: " + handler, EUDPBoxLogType.Log);

            mHandlerList.Add(handler);

            handler.OnRegistedToUDPBox(this);
        }

        public void UnregistHandler(HandlerBase handler)
        {
            Logger.Log("UnregistHandler: " + handler, EUDPBoxLogType.Log);

            mHandlerList.Remove(handler);

            handler.OnUnregistedFromUDPBox(this);
        }

        public void SendMessage(byte[] bytes, IPEndPoint endPoint)
        {
            lock (mSendQueue)
            {
                Logger.Log("SendMessage: " + bytes.Length + " endPoint: " + endPoint, EUDPBoxLogType.Log);

                mSendQueue.Enqueue(new QueueInfo()
                {
                    Content = bytes,
                    IPEndPointAddress_Str = endPoint.Address.ToString(),
                    IPEndPoint_Port = endPoint.Port,
                });
            }
            OnSendMessage?.Invoke(bytes, endPoint);
        }

        public void ProcessPackage(byte[] bytes, IPEndPoint ipEndPoint)
        {
            Logger.Log("Process Package Begin: " + bytes.Length + " ipEndPoint: " + ipEndPoint, EUDPBoxLogType.Log);

            if (StatisticsTotalPackageCount == uint.MaxValue)
            {
                StatisticsBadPackageCount = 0;
                StatisticsTotalPackageCount = 0;
            }

            StatisticsTotalPackageCount++;

            if (UDPBoxUtility.PackageIsBroken(bytes, mPackageHeadBytes))
            {
                StatisticsBadPackageCount++;
                Logger.Log("Package Is Broken!", EUDPBoxLogType.Warning);

                return;
            }

            var flag = false;
            for (int i = 0, iMax = mMessageInterceptList.Count; i < iMax; i++)
            {
                var item = mMessageInterceptList[i];
                flag = item(new MessageInterceptInfo() { Bytes = bytes, IPEndPoint = ipEndPoint });
                if (flag)
                {
                    Logger.Log("Message Intercepted: " + item + " bytes: " + bytes.Length + " ipEndPoint: " + ipEndPoint, EUDPBoxLogType.Log);
                }
            }
            if (flag) return;

            var targetHandler = default(HandlerBase);
            for (int i = 0, iMax = mHandlerList.Count; i < iMax; i++)
            {
                var item = mHandlerList[i];

                var id = UDPBoxUtility.GetPackageID(bytes, mPackageHeadBytes);

                if (UDPBoxUtility.ComparePackageID(bytes, mPackageHeadBytes, item.ProcessableID))
                {
                    targetHandler = item;
                    break;
                }
            }

            Logger.Log("Process Package: " + targetHandler, EUDPBoxLogType.Log);

            if (targetHandler != null)
                targetHandler.Process(this, bytes, ipEndPoint);
        }

        public void Dispose()
        {
            Logger.Log("Dispose", EUDPBoxLogType.Log);

            mACKRequestProcessor.Release();

            if (mSendMessageThread.IsAlive)
                mSendMessageThread.Abort();

            if (mWorkThread.IsAlive)
                mWorkThread.Abort();

            for (int i = 0, iMax = mUdpClientsList.Count; i < iMax; i++)
            {
                var udpClient = mUdpClientsList[i];

                udpClient.Close();
                udpClient.Dispose();
            }

            mIsReleased = true;

            mRecvQueue.Clear();
            mSendQueue.Clear();
            mWorkThreadOperateList.Clear();

            for (int i = mHandlerList.Count - 1; i >= 0; i--)
                UnregistHandler(mHandlerList[i]);
            mHandlerList.Clear();
        }

        UDPBox_UDPClient GetRandomUDPClient()
        {
            return mUdpClientsList[mRandom.Next(0, mUdpClientsList.Count)];
        }

        void SendMessageThreadLoop()
        {
            try
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

                while (!mIsReleased)
                {
                    lock (mSendQueue)
                    {
                        for (int i = 0; i < mSendQueue.Count; i++)
                        {
                            var item = mSendQueue.Dequeue();

                            ipEndPoint.Address = IPAddress.Parse(item.IPEndPointAddress_Str);
                            ipEndPoint.Port = item.IPEndPoint_Port;

                            short type = 0;
                            ushort magicNumber = 0;
                            short id = 0;
                            UDPBoxUtility.GetPackageBaseInfo(item.Content, PackageHeadBytes, out type, out magicNumber, out id);
                            Logger.Log("Final Send Msg: " + id + " magic num: " + magicNumber + " type: " + type, EUDPBoxLogType.Log);
                            GetRandomUDPClient().Send(item.Content, item.Content.Length, ipEndPoint);
                        }
                    }

                    Thread.Sleep(SendMsgThreadSleepTime);
                }
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                OnException?.Invoke(e);
            }
        }

        void ReceiveMessageCallback(IAsyncResult asyncResult)
        {
            var udpClient = asyncResult.AsyncState as UDPBox_UDPClient;
            mCacheIPEndPoint = mCacheIPEndPoint ?? new IPEndPoint(IPAddress.Any, 0);
            var bytes = udpClient.EndReceive(asyncResult, ref mCacheIPEndPoint);

            if (bytes != null)
            {
                lock (mRecvQueue)
                {
                    Logger.Log("Recv msg to queue: " + bytes.Length + " IPEndPoint: " + mCacheIPEndPoint, EUDPBoxLogType.Log);
                    mRecvQueue.Enqueue(new QueueInfo()
                    {
                        Content = bytes,
                        IPEndPointAddress_Str = mCacheIPEndPoint.Address.ToString(),
                        IPEndPoint_Port = mCacheIPEndPoint.Port,
                    });
                }
            }

            udpClient.BeginReceive(ReceiveMessageCallback, udpClient);
        }

        void WorkThreadLoop()
        {
            try
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                while (!mIsReleased)
                {
                    for (int i = 0, iMax = mWorkThreadOperateList.Count; i < iMax; i++)
                    {
                        mWorkThreadOperateList[i]();
                    }

                    lock (mRecvQueue)
                    {
                        for (int i = 0; i < mRecvQueue.Count; i++)
                        {
                            var item = mRecvQueue.Dequeue();

                            ipEndPoint.Address = IPAddress.Parse(item.IPEndPointAddress_Str);
                            ipEndPoint.Port = item.IPEndPoint_Port;

                            ProcessPackage(item.Content, ipEndPoint);
                        }
                    }

                    Thread.Sleep(1);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                OnException?.Invoke(e);
            }
        }
    }
}
