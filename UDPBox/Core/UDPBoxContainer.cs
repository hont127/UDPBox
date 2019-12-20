using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

namespace Hont.UDPBoxPackage
{
    public class UDPBoxContainer
    {
        public const int MAX_CLIENT = 16;

        public const float TIME_OUT_CONNECT_FLOAT = 10f;
        public const float TIME_OUT_NOT_ESTABLISH_CONNECT_FLOAT = 5f;
        public const float PING_CONNECT_TIME_FLOAT = 2f;

        public struct ConnectInfo
        {
            public IPEndPoint IPEndPoint { get; set; }
            public int BeginPort { get; set; }
            public int EndPort { get; set; }
            public float AliveTimer { get; set; }
            public bool IsClientEstablished { get; set; }

            public bool Valid { get { return IPEndPoint != null; } }


            public bool IsPortAndIPContain(IPEndPoint compare)
            {
                var flag = compare.Address.Equals(IPEndPoint.Address);
                flag &= compare.Port >= BeginPort;
                flag &= compare.Port < EndPort;

                return flag;
            }

            public void UpdateToRandomPort(Random random)
            {
                IPEndPoint.Port = random.Next(BeginPort, EndPort);
            }

            public override string ToString()
            {
                return (!Valid) ? "INVALID" : $"IPEndPoint:{IPEndPoint},BeginPort:{BeginPort},EndPort:{EndPort},AliveTimer:{AliveTimer}";
            }
        }

        public enum EState { NoStart, NoClients, HasClients, NoServer, HasServer, Released }

        UDPBox_UDPClient mBroadcastUdpClient;
        UDPBoxBroadcast mUDPBoxBroadcast;

        long mLastWorkThreadTime;
        float mPingTargetThreadTimer;

        bool mUseInternalBroadcastLogic;

        EstablishConnectPackage mEstablishConnectPackage;
        PingPongPackage mPingPongPackageTemplate;

        Random mRandom;

        public EState State { get; private set; }
        public IPAddress SelfIPAddress { get; private set; }
        public ConnectInfo MasterIPConnectInfo { get; private set; }
        public List<ConnectInfo> ClientIPConnectList { get; private set; } = new List<ConnectInfo>(MAX_CLIENT);

        public bool IsMaster { get; private set; }

        public int BroadcastListenPort { get; set; } = 1234;
        public int BroadcastSendPort { get; set; } = 1235;
        public int UdpBoxBeginPort { get; set; } = 1240;
        public int UdpBoxEndPort { get; set; } = 1250;

        public int SendMsgThreadSleepTime
        {
            get { return UDPBox.SendMsgThreadSleepTime; }
            set { UDPBox.SendMsgThreadSleepTime = value; }
        }

        public uint Statistics_BadPackageCount { get { return UDPBox.StatisticsBadPackageCount; } }
        public uint Statistics_TotalPackageCount { get { return UDPBox.StatisticsTotalPackageCount; } }
        public string BroadcastNetPrefixIP { get; set; } = "192.168.1.";

        public UDPBox UDPBox { get; private set; }
        public byte[] PackageHeadBytes { get { return UDPBox.PackageHeadBytes; } }

        public event Action OnConnectedMaster;
        public event Action OnConnectedClient;

        public event Action OnMasterIPTimeOut;
        public event Action<IPEndPoint> OnClientTimeOut;

        public event Action<Exception> OnException
        {
            add { UDPBox.OnException += value; }
            remove { UDPBox.OnException -= value; }
        }


        public UDPBoxContainer()
        {
            State = EState.NoStart;
            mRandom = new System.Random();
            UDPBox = new UDPBox();
        }

        public void Initialization(string packageHead, bool useInternalBroadcastLogic)
        {
            mUseInternalBroadcastLogic = useInternalBroadcastLogic;

            var udpClientArray = new UDPBox_UDPClient[UdpBoxEndPort - UdpBoxBeginPort];
            for (int i = 0, port = UdpBoxBeginPort; port < UdpBoxEndPort; i++, port++)
                udpClientArray[i] = UDPBoxFactory.GeterateUdpClient(port);
            UDPBox.Initialization(udpClientArray, packageHead);

            mEstablishConnectPackage = new EstablishConnectPackage(PackageHeadBytes);
            mPingPongPackageTemplate = new PingPongPackage(PackageHeadBytes);
            SelfIPAddress = SelfIPAddress ?? UDPBoxUtility.GetSelfIP(BroadcastNetPrefixIP);
        }

        public void RestartUDPBoxContainer(string packageHead, bool useInternalBroadcastLogic, bool isMaster)
        {
            if (State != EState.NoStart)
                Release();

            Initialization(packageHead, useInternalBroadcastLogic);
            Start(isMaster);
        }

        public void Start(bool isMaster)
        {
            IsMaster = isMaster;

            if (mUseInternalBroadcastLogic)
            {
                mBroadcastUdpClient = UDPBoxFactory.GeterateUdpClient(BroadcastListenPort);
                mUDPBoxBroadcast = UDPBoxFactory.GenerateStandardUDPBoxBroadcastAndSetup(mBroadcastUdpClient, BroadcastSendPort, BroadcastNetPrefixIP, this);
            }

            UDPBox.RegistMessageIntercept(InterceptAndUpdateConnectState);
            UDPBox.RegistWorkThreadOperate(RefreshConnectStateInWorkThread);
            UDPBox.Start();
        }

        public void Release()
        {
            State = EState.Released;
            if (mUseInternalBroadcastLogic)
            {
                mBroadcastUdpClient.Close();
                mBroadcastUdpClient.Dispose();
                mUDPBoxBroadcast?.ReleaseThread();
            }
            UDPBox.UnregistMessageIntercept(InterceptAndUpdateConnectState);
            UDPBox.UnregistWorkThreadOperate(RefreshConnectStateInWorkThread);
            UDPBox.Dispose();
        }

        public void SetLogger(IUDPBoxLogger logger)
        {
            UDPBox.SetLogger(logger);
        }

        public void SendUDPMessageToRandomPort(byte[] bytes, ConnectInfo connectInfo)
        {
            UpdateConnectInfoToRandomPort(connectInfo);
            SendUDPMessage(bytes, connectInfo.IPEndPoint);
        }

        public void SendUDPMessageToRandomPort(byte[] bytes, int beginPort, int endPort, IPAddress address)
        {
            var port = mRandom.Next(beginPort, endPort);
            var ipEndPoint = new IPEndPoint(address, port);
            SendUDPMessage(bytes, ipEndPoint);
        }

        public void SendUDPMessageToRandomPort(byte[] bytes, int beginPort, int endPort, IPEndPoint endPoint)
        {
            endPoint.Port = mRandom.Next(beginPort, endPort);
            SendUDPMessage(bytes, endPoint);
        }

        public void SendUDPMessage(byte[] bytes, IPEndPoint ipEndPoint)
        {
            UDPBox.SendMessage(bytes, ipEndPoint);
        }

        public void RegistHandler(HandlerBase handler)
        {
            UDPBox.RegistHandler(handler);
        }

        public void UnregistHandler(HandlerBase handler)
        {
            UDPBox.RegistHandler(handler);
        }

        public void RegistWorkThreadOperate(Action operateAction)
        {
            UDPBox.RegistWorkThreadOperate(operateAction);
        }

        public void UnregistWorkThreadOperate(Action operateAction)
        {
            UDPBox.UnregistWorkThreadOperate(operateAction);
        }

        public int GetRandomUDPBoxPort()
        {
            return mRandom.Next(UdpBoxBeginPort, UdpBoxEndPort);
        }

        public ConnectInfo UpdateConnectInfoToRandomPort(ConnectInfo connectInfo)
        {
            connectInfo.UpdateToRandomPort(mRandom);

            return connectInfo;
        }

        void RefreshConnectStateInWorkThread()
        {
            var deltaTime = UDPBoxUtility.GetDeltaTime(mLastWorkThreadTime);

            if (IsMaster)
            {
                if (ClientIPConnectList.Count > 0)
                    State = EState.HasClients;
                else
                    State = EState.NoClients;

                for (int i = 0, iMax = ClientIPConnectList.Count; i < iMax; i++)
                {
                    var item = ClientIPConnectList[i];

                    if (!item.Valid) continue;

                    item = new ConnectInfo()
                    {
                        AliveTimer = item.AliveTimer + deltaTime,
                        IPEndPoint = item.IPEndPoint,
                        BeginPort = item.BeginPort,
                        EndPort = item.EndPort,
                        IsClientEstablished = item.IsClientEstablished,
                    };

                    ClientIPConnectList[i] = item;
                }
            }
            else
            {
                if (MasterIPConnectInfo.Valid)
                {
                    State = EState.HasServer;

                    MasterIPConnectInfo = new ConnectInfo()
                    {
                        AliveTimer = MasterIPConnectInfo.AliveTimer + deltaTime,
                        IPEndPoint = MasterIPConnectInfo.IPEndPoint,
                        BeginPort = MasterIPConnectInfo.BeginPort,
                        EndPort = MasterIPConnectInfo.EndPort,
                        IsClientEstablished = MasterIPConnectInfo.IsClientEstablished,
                    };
                }
                else
                {
                    State = EState.NoServer;
                }
            }
            //update ticks.

            if (IsMaster)
            {
                for (int i = ClientIPConnectList.Count - 1; i >= 0; i--)
                {
                    var item = ClientIPConnectList[i];

                    if (!item.Valid) continue;

                    if (!item.IsClientEstablished && item.AliveTimer > TIME_OUT_NOT_ESTABLISH_CONNECT_FLOAT)
                    {
                        ClientIPConnectList.RemoveAt(i);
                    }
                    else if (item.AliveTimer > TIME_OUT_CONNECT_FLOAT)
                    {
                        ClientIPConnectList.RemoveAt(i);
                        OnClientTimeOut?.Invoke(item.IPEndPoint);
                    }
                }
            }
            else
            {
                if (MasterIPConnectInfo.Valid)
                {
                    if (MasterIPConnectInfo.AliveTimer > TIME_OUT_CONNECT_FLOAT)
                    {
                        MasterIPConnectInfo = new ConnectInfo();
                        OnMasterIPTimeOut?.Invoke();
                    }
                }
            }
            //update timeout.

            if (mPingTargetThreadTimer <= 0f)
            {
                mPingPongPackageTemplate.PingPong = PingPongPackage.EPingPong.Ping;

                if (IsMaster)
                {
                    for (int i = 0, iMax = ClientIPConnectList.Count; i < iMax; i++)
                    {
                        var item = ClientIPConnectList[i];
                        if (item.Valid && item.IsClientEstablished)
                        {
                            SendUDPMessageToRandomPort(mPingPongPackageTemplate.Serialize(), item);
                        }
                    }
                }
                else
                {
                    if (MasterIPConnectInfo.Valid)
                    {
                        SendUDPMessageToRandomPort(mPingPongPackageTemplate.Serialize(), MasterIPConnectInfo);
                    }
                }

                mPingTargetThreadTimer = PING_CONNECT_TIME_FLOAT;
            }
            else
            {
                mPingTargetThreadTimer -= deltaTime;
            }//ping connect.

            mLastWorkThreadTime = DateTime.Now.Ticks;
        }

        bool InterceptAndUpdateConnectState(UDPBox.MessageInterceptInfo messageInterceptInfo)
        {
            if (UDPBoxUtility.PackageIsBroken(messageInterceptInfo.Bytes, PackageHeadBytes)) return true;

            var packageID = UDPBoxUtility.GetPackageID(messageInterceptInfo.Bytes, PackageHeadBytes);

            if (packageID == UDPBoxUtility.ESTABLISH_CONNECT_ID)
            {
                if (!mEstablishConnectPackage.Deserialize(messageInterceptInfo.Bytes)) return true;

                var senderType = mEstablishConnectPackage.SenderType;
                var establishIPAddress = mEstablishConnectPackage.IpAddress;
                var packageBeginPort = mEstablishConnectPackage.BeginPort;
                var packageEndPort = mEstablishConnectPackage.EndPort;
                var isReceipt = mEstablishConnectPackage.IsReceipt;

                if (senderType == EstablishConnectPackage.ESenderType.Server)
                {
                    MasterIPConnectInfo = new ConnectInfo()
                    {
                        IPEndPoint = new IPEndPoint(IPAddress.Parse(establishIPAddress), packageBeginPort),
                        BeginPort = packageBeginPort,
                        EndPort = packageEndPort,
                    };
                    OnConnectedMaster?.Invoke();

                    if (!isReceipt)
                    {
                        mEstablishConnectPackage.SenderType = EstablishConnectPackage.ESenderType.Client;
                        mEstablishConnectPackage.IpAddress = SelfIPAddress.ToString();
                        mEstablishConnectPackage.BeginPort = UdpBoxBeginPort;
                        mEstablishConnectPackage.EndPort = UdpBoxEndPort;
                        mEstablishConnectPackage.IsReceipt = true;
                        SendUDPMessageToRandomPort(mEstablishConnectPackage.Serialize(), packageBeginPort, packageEndPort, IPAddress.Parse(establishIPAddress));
                    }
                }
                else if (senderType == EstablishConnectPackage.ESenderType.Client)
                {
                    var connectInfo = new ConnectInfo()
                    {
                        IPEndPoint = new IPEndPoint(IPAddress.Parse(establishIPAddress), packageBeginPort),
                        BeginPort = packageBeginPort,
                        EndPort = packageEndPort,
                    };

                    if (ClientIPConnectList.Count < MAX_CLIENT
                        && !ClientIPConnectList.Find(m => m.IsPortAndIPContain(connectInfo.IPEndPoint)).Valid)
                    {
                        ClientIPConnectList.Add(connectInfo);
                        OnConnectedClient?.Invoke();
                    }

                    if (!isReceipt)
                    {
                        mEstablishConnectPackage.SenderType = EstablishConnectPackage.ESenderType.Server;
                        mEstablishConnectPackage.IpAddress = SelfIPAddress.ToString();
                        mEstablishConnectPackage.BeginPort = UdpBoxBeginPort;
                        mEstablishConnectPackage.EndPort = UdpBoxEndPort;
                        mEstablishConnectPackage.IsReceipt = true;
                        SendUDPMessageToRandomPort(mEstablishConnectPackage.Serialize(), packageBeginPort, packageEndPort, IPAddress.Parse(establishIPAddress));
                    }
                }
            }
            else if (packageID == UDPBoxUtility.PING_PONG_ID)
            {
                if (!mPingPongPackageTemplate.Deserialize(messageInterceptInfo.Bytes)) return true;
                if (mPingPongPackageTemplate.PingPong == PingPongPackage.EPingPong.Pong) return true;

                if (IsMaster)
                {
                    for (int i = 0, iMax = ClientIPConnectList.Count; i < iMax; i++)
                    {
                        var clientEndPoint = ClientIPConnectList[i];

                        if (clientEndPoint.IsPortAndIPContain(messageInterceptInfo.IPEndPoint))
                        {
                            ClientIPConnectList[i] = new ConnectInfo()
                            {
                                AliveTimer = 0f,
                                IPEndPoint = clientEndPoint.IPEndPoint,
                                BeginPort = clientEndPoint.BeginPort,
                                EndPort = clientEndPoint.EndPort,
                                IsClientEstablished = true
                            };
                        }
                    }
                }
                else
                {
                    if (MasterIPConnectInfo.Valid && MasterIPConnectInfo.IsPortAndIPContain(messageInterceptInfo.IPEndPoint))
                    {
                        MasterIPConnectInfo = new ConnectInfo()
                        {
                            AliveTimer = 0f,
                            IPEndPoint = messageInterceptInfo.IPEndPoint,
                            BeginPort = MasterIPConnectInfo.BeginPort,
                            EndPort = MasterIPConnectInfo.EndPort,
                        };
                    }
                }
                return true;
            }

            return false;
        }
    }
}
