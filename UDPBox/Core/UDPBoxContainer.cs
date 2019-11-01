using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

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
            public float AliveTimer { get; set; }
            public bool IsClientEstablished { get; set; }

            public bool Valid { get { return IPEndPoint != null; } }
        }

        public enum EState { NoStart, NoClients, HasClients, NoServer, HasServer, Released }

        UdpClient mBroadcastUdpClient;
        UDPBoxBroadcast mUDPBoxBroadcast;

        long mLastWorkThreadTime;
        float mPingTargetThreadTimer;

        BroadcastPackage mBroadcastPackageTemplate;
        PingPongPackage mPingPongPackageTemplate;
        EstablishServerConnectPackage mEstablishServerConnectPackage;

        public EState State { get; private set; }
        public IPAddress SelfIPAddress { get; private set; }
        public ConnectInfo MasterIPConnectInfo { get; private set; }
        public List<ConnectInfo> ClientIPConnectList { get; private set; } = new List<ConnectInfo>(MAX_CLIENT);

        public string Proj_Prefix { get; set; } = "Demo";
        public bool IsMaster { get; private set; }

        public int BroadcastListenPort { get; set; } = 1234;
        public int BroadcastSendPort { get; set; } = 1235;
        public int UdpBoxPort { get; set; } = 1236;

        public int ReceiveMsgThreadSleepTime
        {
            get { return UDPBox.ReceiveMsgThreadSleepTime; }
            set { UDPBox.ReceiveMsgThreadSleepTime = value; }
        }
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


        public UDPBoxContainer()
        {
            State = EState.NoStart;
        }

        public void Initialization()
        {
            var udpClient = UDPBoxUtility.GeterateUdpClient(UdpBoxPort);
            UDPBox = new UDPBox(udpClient, UDPBoxUtility.DefaultHead);
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

                var ipEndPoint = new IPEndPoint(IPAddress.Parse(mBroadcastPackageTemplate.IpAddress), mBroadcastPackageTemplate.Port);

                if (ipEndPoint.Address.Equals(SelfIPAddress) && ipEndPoint.Port == UdpBoxPort) return;

                if (isMaster)
                {
                    if (ClientIPConnectList.Count < MAX_CLIENT && !ClientIPConnectList.Find(m => m.IPEndPoint.Equals(ipEndPoint)).Valid)
                    {
                        ClientIPConnectList.Add(new ConnectInfo() { IPEndPoint = ipEndPoint });
                        mEstablishServerConnectPackage.IpAddress = SelfIPAddress.ToString();
                        mEstablishServerConnectPackage.Port = UdpBoxPort;
                        UDPBox.SendMessage(mEstablishServerConnectPackage.Serialize(), ipEndPoint);

                        OnConnectedClient?.Invoke(ipEndPoint);
                    }
                }
                else
                {
                    if (!MasterIPConnectInfo.Valid)
                    {
                        Debug.LogError("Init!!!! self ip" + SelfIPAddress + " ipEndPoint.Address : " + ipEndPoint.Address + " self port: " + ipEndPoint.Port + " udpbox port: " + UdpBoxPort);
                        MasterIPConnectInfo = new ConnectInfo() { IPEndPoint = ipEndPoint };

                        OnConnectedMaster?.Invoke();
                    }
                }
            });

            if (!IsMaster)
            {
                mBroadcastPackageTemplate.ProjectPrefix = Proj_Prefix;
                mBroadcastPackageTemplate.IpAddress = SelfIPAddress.ToString();
                mBroadcastPackageTemplate.Port = UdpBoxPort;

                mUDPBoxBroadcast.StartBroadcast(mBroadcastPackageTemplate.Serialize(), BroadcastNetPrefixIP);
            }

            UDPBox.OnMessageIntercept += InterceptAndUpdateConnectState;
            UDPBox.RegistWorkThreadOperate(RefreshConnectStateInWorkThread);
            UDPBox.Start();
        }

        public void Release()
        {
            State = EState.Released;
            mBroadcastUdpClient.Close();
            mBroadcastUdpClient.Dispose();
            UDPBox.OnMessageIntercept -= InterceptAndUpdateConnectState;
            UDPBox.UnregistWorkThreadOperate(RefreshConnectStateInWorkThread);
            mUDPBoxBroadcast?.ReleaseThread();
            UDPBox.Dispose();
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

        void RefreshConnectStateInWorkThread()
        {
            var deltaTime = UDPBoxUtility.GetDeltaTime(mLastWorkThreadTime);

            lock (this)
            {
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

                        if (item.AliveTimer > TIME_OUT_CONNECT_FLOAT)
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
                                UDPBox.SendMessage(mPingPongPackageTemplate.Serialize(), item.IPEndPoint);
                        }
                    }
                    else
                    {
                        if (MasterIPConnectInfo.Valid)
                            UDPBox.SendMessage(mPingPongPackageTemplate.Serialize(), MasterIPConnectInfo.IPEndPoint);
                    }

                    mPingTargetThreadTimer = PING_CONNECT_TIME_FLOAT;
                }
                else
                {
                    mPingTargetThreadTimer -= deltaTime;
                }//ping connect.
            }

            mLastWorkThreadTime = DateTime.Now.Ticks;
        }

        bool InterceptAndUpdateConnectState(byte[] bytes, IPEndPoint ipEndPoint)
        {
            if (UDPBoxUtility.PackageIsBroken(bytes, PackageHeadBytes)) return true;

            var packageID = UDPBoxUtility.GetPackageID(bytes, PackageHeadBytes);

            if (packageID == UDPBoxUtility.ESTABLISH_SERVER_CONNECT_ID)
            {
                if (!mEstablishServerConnectPackage.Deserialize(bytes)) return false;

                var establishIPAddress = mEstablishServerConnectPackage.IpAddress;
                var establishPort = mEstablishServerConnectPackage.Port;

                if (!IsMaster)
                {
                    MasterIPConnectInfo = new ConnectInfo()
                    {
                        IPEndPoint = new IPEndPoint(IPAddress.Parse(establishIPAddress), establishPort)
                    };
                }
            }
            else if (packageID == UDPBoxUtility.PING_PONG_ID)
            {
                if (!mPingPongPackageTemplate.Deserialize(bytes)) return false;
                if (mPingPongPackageTemplate.PingPong == PingPongPackage.EPingPong.Ping) return false;

                lock (this)
                {
                    if (IsMaster)
                    {
                        for (int i = 0, iMax = ClientIPConnectList.Count; i < iMax; i++)
                        {
                            var clientEndPoint = ClientIPConnectList[i];
                            if (clientEndPoint.IPEndPoint.Equals(ipEndPoint))
                            {
                                ClientIPConnectList[i] = new ConnectInfo() { AliveTimer = 0f, IPEndPoint = clientEndPoint.IPEndPoint, IsClientEstablished = true };
                            }
                        }
                    }
                    else
                    {
                        if (MasterIPConnectInfo.Valid && MasterIPConnectInfo.IPEndPoint.Equals(ipEndPoint))
                        {
                            MasterIPConnectInfo = new ConnectInfo() { AliveTimer = 0f, IPEndPoint = ipEndPoint };
                        }
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
