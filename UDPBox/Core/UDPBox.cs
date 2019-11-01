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

        UdpClient mUdpClient;
        byte[] mPackageHeadBytes;
        List<HandlerBase> mHandlerList;
        List<Action> mWorkThreadOperateList;

        Thread mSendMessageThread;
        Thread mReceiveMessageLoopThread;
        Thread mWorkThread;

        bool mIsReleased;

        Queue<QueueInfo> mSendQueue;
        Queue<QueueInfo> mRecvQueue;

        ACKRequestProcessor mACKRequestProcessor;

        public int ReceiveMsgThreadSleepTime { get; set; } = 11;
        public int SendMsgThreadSleepTime { get; set; } = 7;

        public uint StatisticsBadPackageCount { get; private set; }
        public uint StatisticsTotalPackageCount { get; private set; }

        public event Func<byte[], IPEndPoint, bool> OnMessageIntercept;
        public event Action<byte[], IPEndPoint> OnSendMessage;

        public byte[] PackageHeadBytes { get { return mPackageHeadBytes; } }


        public UDPBox(UdpClient udpClient, string packageHead)
        {
            mUdpClient = udpClient;
            mPackageHeadBytes = UDPBoxUtility.ToBuffer(packageHead);

            mACKRequestProcessor = new ACKRequestProcessor(this);

            mSendQueue = new Queue<QueueInfo>(32);
            mRecvQueue = new Queue<QueueInfo>(32);

            mHandlerList = new List<HandlerBase>(16);
            mWorkThreadOperateList = new List<Action>(16);

            RegistHandler(new PingPongHandler());

            mReceiveMessageLoopThread = new Thread(ReceiveMessageThreadLoop);
            mReceiveMessageLoopThread.Priority = ThreadPriority.AboveNormal;
            mSendMessageThread = new Thread(SendMessageThreadLoop);
            mSendMessageThread.Priority = ThreadPriority.AboveNormal;
            mWorkThread = new Thread(WorkThreadLoop);
        }

        public void Start()
        {
            mACKRequestProcessor.Initialization();

            mIsReleased = false;
            mReceiveMessageLoopThread.Start();
            mSendMessageThread.Start();
            mWorkThread.Start();
        }

        public void RegistWorkThreadOperate(Action operateAction)
        {
            mWorkThreadOperateList.Add(operateAction);
        }

        public void UnregistWorkThreadOperate(Action operateAction)
        {
            mWorkThreadOperateList.Remove(operateAction);
        }

        public void RegistHandler(HandlerBase handler)
        {
            mHandlerList.Add(handler);

            handler.OnRegistedToUDPBox(this);
        }

        public void UnregistHandler(HandlerBase handler)
        {
            mHandlerList.Remove(handler);

            handler.OnUnregistedFromUDPBox(this);
        }

        public void SendMessage(byte[] bytes, IPEndPoint endPoint)
        {
            mSendQueue.Enqueue(new QueueInfo()
            {
                Content = bytes,
                IPEndPointAddress_Str = endPoint.Address.ToString(),
                IPEndPoint_Port = endPoint.Port,
            });
            OnSendMessage?.Invoke(bytes, endPoint);
        }

        public void Dispose()
        {
            mACKRequestProcessor.Release();

            if (mSendMessageThread.IsAlive)
                mSendMessageThread.Abort();

            if (mReceiveMessageLoopThread.IsAlive)
                mReceiveMessageLoopThread.Abort();

            if (mWorkThread.IsAlive)
                mWorkThread.Abort();

            mUdpClient.Close();
            mUdpClient.Dispose();

            mIsReleased = true;

            mRecvQueue.Clear();
            mSendQueue.Clear();
            mWorkThreadOperateList.Clear();

            for (int i = mHandlerList.Count - 1; i >= 0; i--)
                UnregistHandler(mHandlerList[i]);
            mHandlerList.Clear();
        }

        void SendMessageThreadLoop()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (!mIsReleased)
            {
                for (int i = 0; i < mSendQueue.Count; i++)
                {
                    var item = mSendQueue.Dequeue();

                    ipEndPoint.Address = IPAddress.Parse(item.IPEndPointAddress_Str);
                    ipEndPoint.Port = item.IPEndPoint_Port;

                    mUdpClient.Send(item.Content, item.Content.Length, ipEndPoint);
                }

                Thread.Sleep(SendMsgThreadSleepTime);
            }
        }

        void ReceiveMessageThreadLoop()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (!mIsReleased)
            {
                var bytes = default(byte[]);

                try
                {
                    lock (mUdpClient)
                    {
                        ipEndPoint.Address = IPAddress.Any;
                        ipEndPoint.Port = 0;
                        bytes = mUdpClient.Receive(ref ipEndPoint);
                    }
                }
                catch (SocketException socketException) when (socketException.ErrorCode == 10060)
                {
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e);
                }

                if (bytes != null)
                {
                    mRecvQueue.Enqueue(new QueueInfo()
                    {
                        Content = bytes,
                        IPEndPointAddress_Str = ipEndPoint.Address.ToString(),
                        IPEndPoint_Port = ipEndPoint.Port,
                    });
                }

                Thread.Sleep(ReceiveMsgThreadSleepTime);
            }
        }

        void WorkThreadLoop()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (!mIsReleased)
            {
                for (int i = 0, iMax = mWorkThreadOperateList.Count; i < iMax; i++)
                {
                    mWorkThreadOperateList[i]();
                }

                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                for (int i = 0; i < mRecvQueue.Count; i++)
                {
                    var item = mRecvQueue.Dequeue();

                    ipEndPoint.Address = IPAddress.Parse(item.IPEndPointAddress_Str);
                    ipEndPoint.Port = item.IPEndPoint_Port;

                    ProcessPackage(item.Content, ipEndPoint);
                }

                stopwatch.Stop();

                Thread.Sleep(1);
            }
        }

        void ProcessPackage(byte[] bytes, IPEndPoint ipEndPoint)
        {
            if (StatisticsTotalPackageCount == uint.MaxValue)
            {
                StatisticsBadPackageCount = 0;
                StatisticsTotalPackageCount = 0;
            }

            StatisticsTotalPackageCount++;

            if (UDPBoxUtility.PackageIsBroken(bytes, mPackageHeadBytes))
            {
                StatisticsBadPackageCount++;

                return;
            }

            if (OnMessageIntercept != null && OnMessageIntercept(bytes, ipEndPoint))
                return;

            var targetHandler = default(HandlerBase);
            for (int i = 0, iMax = mHandlerList.Count; i < iMax; i++)
            {
                var item = mHandlerList[i];
                if (UDPBoxUtility.ComparePackageID(bytes, mPackageHeadBytes, item.ProcessableID))
                {
                    targetHandler = item;
                    break;
                }
            }

            if (targetHandler != null)
                targetHandler.Process(this, bytes, ipEndPoint);
        }
    }
}
