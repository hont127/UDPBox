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

        UdpClient mBroadcastUdpClient;
        UDPBoxBroadcast mUDPBoxBroadcast;

        long mLastWorkThreadTime;
        float mPingTargetThreadTimer;

        BroadcastPackage mBroadcastPackageTemplate;
        PingPongPackage mPingPongPackageTemplate;
        EstablishServerConnectPackage mEstablishServerConnectPackage;

        Random mRandom;

        public EState State { get; private set; }
        public IPAddress SelfIPAddress { get; private set; }
        public ConnectInfo MasterIPConnectInfo { get; private set; }
        public List<ConnectInfo> ClientIPConnectList { get; private set; } = new List<ConnectInfo>(MAX_CLIENT);

        public string Proj_Prefix { get; set; } = "Demo";
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
        public event Action<IPEndPoint> OnConnectedClient;

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

        public void Initialization(string packageHead)
        {
            var udpClientArray = new UdpClient[UdpBoxEndPort - UdpBoxBeginPort];
            for (int i = 0, port = UdpBoxBeginPort; port < UdpBoxEndPort; i++, port++)
            {
                udpClientArray[i] = UDPBoxUtility.GeterateUdpClient(port);
            }

            UDPBox.Initialization(udpClientArray, packageHead);

            mBroadcastPackageTemplate = new BroadcastPackage(PackageHeadBytes);
            mPingPongPackageTemplate = new PingPongPackage(PackageHeadBytes);
            mEstablishServerConnectPackage = new EstablishServerConnectPackage(PackageHeadBytes);
            SelfIPAddress = SelfIPAddress ?? UDPBoxUtility.GetSelfIP(BroadcastNetPrefixIP);
        }

        public void Start(bool isMaster)
        {
            IsMaster = isMaster;

            mBroadcastUdpClient = UDPBoxUtility.GeterateUdpClient(BroadcastListenPort);
            mUDPBoxBroadcast = new UDPBoxBroadcast(mBroadcastUdpClient, BroadcastSendPort);

            mUDPBoxBroadcast.ListenBroadcast((bytes, endPoint) =>
            {
                if (UDPBoxUtility.PackageIsBroken(bytes, PackageHeadBytes)) return;
                if (!UDPBoxUtility.ComparePackageID(bytes, PackageHeadBytes, mBroadcastPackageTemplate.ID)) return;

                if (!mBroadcastPackageTemplate.Deserialize(bytes)) return;
                if (mBroadcastPackageTemplate.ProjectPrefix != Proj_Prefix) return;

                var ipEndPoint = new IPEndPoint(IPAddress.Parse(mBroadcastPackageTemplate.IpAddress), mBroadcastPackageTemplate.BeginPort);
                if (ipEndPoint.Address.Equals(SelfIPAddress)
                    && ipEndPoint.Port < UdpBoxEndPort && ipEndPoint.Port >= UdpBoxBeginPort) return;
                //Provoid self connect to the self.

                if (isMaster)
                {
                    if (ClientIPConnectList.Count < MAX_CLIENT
                        && !ClientIPConnectList.Find(m => m.IsPortAndIPContain(ipEndPoint)).Valid)
                    {
                        ClientIPConnectList.Add(new ConnectInfo()
                        {
                            IPEndPoint = ipEndPoint,
                            BeginPort = mBroadcastPackageTemplate.BeginPort,
                            EndPort = mBroadcastPackageTemplate.EndPort,
                        });

                        mEstablishServerConnectPackage.IpAddress = SelfIPAddress.ToString();
                        mEstablishServerConnectPackage.BeginPort = UdpBoxBeginPort;
                        mEstablishServerConnectPackage.EndPort = UdpBoxEndPort;
                        UDPBox.SendMessage(mEstablishServerConnectPackage.Serialize(), ipEndPoint);

                        OnConnectedClient?.Invoke(ipEndPoint);
                    }
                }
            });

            if (!IsMaster)
            {
                mBroadcastPackageTemplate.ProjectPrefix = Proj_Prefix;
                mBroadcastPackageTemplate.IpAddress = SelfIPAddress.ToString();
                mBroadcastPackageTemplate.BeginPort = UdpBoxBeginPort;
                mBroadcastPackageTemplate.EndPort = UdpBoxEndPort;

                mUDPBoxBroadcast.StartBroadcast(mBroadcastPackageTemplate.Serialize(), BroadcastNetPrefixIP);
            }

            UDPBox.RegistMessageIntercept(InterceptAndUpdateConnectState);
            UDPBox.RegistWorkThreadOperate(RefreshConnectStateInWorkThread);
            UDPBox.Start();
        }

        public void Release()
        {
            State = EState.Released;
            mBroadcastUdpClient.Close();
            mBroadcastUdpClient.Dispose();
            UDPBox.UnregistMessageIntercept(InterceptAndUpdateConnectState);
            UDPBox.UnregistWorkThreadOperate(RefreshConnectStateInWorkThread);
            mUDPBoxBroadcast?.ReleaseThread();
            UDPBox.Dispose();
        }

        public void SetLogger(IUDPBoxLogger logger)
        {
            UDPBox.SetLogger(logger);
        }

        public void SendUDPMessageWithRandomPort(byte[] bytes, ConnectInfo connectInfo)
        {
            UpdateConnectInfoToRandomPort(connectInfo);
            SendUDPMessage(bytes, connectInfo.IPEndPoint);
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
                        continue;
                    }

                    if (item.AliveTimer <= TIME_OUT_CONNECT_FLOAT) continue;

                    ClientIPConnectList.RemoveAt(i);
                    OnClientTimeOut?.Invoke(item.IPEndPoint);
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
                            SendUDPMessageWithRandomPort(mPingPongPackageTemplate.Serialize(), item);
                        }
                    }
                }
                else
                {
                    if (MasterIPConnectInfo.Valid)
                    {
                        SendUDPMessageWithRandomPort(mPingPongPackageTemplate.Serialize(), MasterIPConnectInfo);
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

            if (packageID == UDPBoxUtility.ESTABLISH_SERVER_CONNECT_ID)
            {
                if (!mEstablishServerConnectPackage.Deserialize(messageInterceptInfo.Bytes)) return true;

                var establishIPAddress = mEstablishServerConnectPackage.IpAddress;
                var serverBeginPort = mEstablishServerConnectPackage.BeginPort;
                var serverEndPort = mEstablishServerConnectPackage.EndPort;

                if (!IsMaster)
                {
                    MasterIPConnectInfo = new ConnectInfo()
                    {
                        IPEndPoint = new IPEndPoint(IPAddress.Parse(establishIPAddress), serverBeginPort),
                        BeginPort = serverBeginPort,
                        EndPort = serverEndPort,
                    };
                    OnConnectedMaster?.Invoke();
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
