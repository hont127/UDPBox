using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    [DefaultExecutionOrder(-5)]
    public class UDPBoxContainer_Mono : MonoBehaviour
    {
        UDPBoxContainer mUDPBoxContainer;
        public bool isMaster;
        public bool isDebug;

        public UDPBoxContainer.ConnectInfo MasterIPConnectInfo { get { return mUDPBoxContainer.MasterIPConnectInfo; } }
        public List<UDPBoxContainer.ConnectInfo> ClientIPConnectInfoList { get { return mUDPBoxContainer.ClientIPConnectList; } }

        public int broadcastSendPort = 1234;
        public int broadcastListenPort = 1235;
        public int udpBoxPort = 1236;
        [Tooltip("format: \"192.168.1.\"")]
        public string broadcastNetprefixIP = "192.168.1.";
        public string proj_prefix = "Demo";
        public int sendMsgThreadSleepTime_MS = 35;
        public int recvMsgThreadSleepTime_MS = 32;

        public UDPBox UDPBox { get { return mUDPBoxContainer.UDPBox; } }
        public UDPBoxContainer.EState State { get { return mUDPBoxContainer.State; } }

        public event Action OnConnectedMaster
        {
            add { mUDPBoxContainer.OnConnectedMaster += value; }
            remove { mUDPBoxContainer.OnConnectedMaster -= value; }
        }
        public event Action<IPEndPoint> OnConnectedClient
        {
            add { mUDPBoxContainer.OnConnectedClient += value; }
            remove { mUDPBoxContainer.OnConnectedClient -= value; }
        }
        public event Action OnMasterIPTimeOut
        {
            add { mUDPBoxContainer.OnMasterIPTimeOut += value; }
            remove { mUDPBoxContainer.OnMasterIPTimeOut -= value; }
        }
        public event Action<IPEndPoint> OnClientTimeOut
        {
            add { mUDPBoxContainer.OnClientTimeOut += value; }
            remove { mUDPBoxContainer.OnClientTimeOut -= value; }
        }


        public void SendUDPMessage(byte[] bytes, IPEndPoint ipEndPoint)
        {
            mUDPBoxContainer.SendUDPMessage(bytes, ipEndPoint);
        }

        public void RegistHandler(HandlerBase handler)
        {
            mUDPBoxContainer.RegistHandler(handler);
        }

        public void UnregistHandler(HandlerBase handler)
        {
            mUDPBoxContainer.RegistHandler(handler);
        }

        public void RegistWorkThreadOperate(Action operateAction)
        {
            mUDPBoxContainer.RegistWorkThreadOperate(operateAction);
        }

        public void UnregistWorkThreadOperate(Action operateAction)
        {
            mUDPBoxContainer.UnregistWorkThreadOperate(operateAction);
        }

        public void RestartUDPBoxContainer()
        {
            mUDPBoxContainer.Proj_Prefix = proj_prefix;
            mUDPBoxContainer.BroadcastListenPort = broadcastListenPort;
            mUDPBoxContainer.BroadcastSendPort = broadcastSendPort;
            mUDPBoxContainer.UdpBoxPort = udpBoxPort;
            mUDPBoxContainer.BroadcastNetPrefixIP = broadcastNetprefixIP;

            mUDPBoxContainer.Initialization();
            mUDPBoxContainer.SendMsgThreadSleepTime = sendMsgThreadSleepTime_MS;
            mUDPBoxContainer.ReceiveMsgThreadSleepTime = recvMsgThreadSleepTime_MS;

            mUDPBoxContainer.Start(isMaster);
        }

        public void ReleaseUDPBoxContainer()
        {
            mUDPBoxContainer.Release();
        }

        public UDPBoxContainer GetNativeContainer()
        {
            return mUDPBoxContainer;
        }

        void OnEnable()
        {
            UDPBox_GameThreadMediator.Instance.GetHashCode();

            mUDPBoxContainer = new UDPBoxContainer();

            if (GetComponent<UDPBoxMasterSearcher>() == null)
                RestartUDPBoxContainer();
        }

        void OnDisable()
        {
            ReleaseUDPBoxContainer();
        }

        void OnGUI()
        {
            if (!isDebug) return;

            if (mUDPBoxContainer.SelfIPAddress != null)
                GUILayout.Box("Self IP:" + mUDPBoxContainer.SelfIPAddress);

            GUILayout.Box("State: " + mUDPBoxContainer.State);
            GUILayout.Box("Bad Package Rate: " + (mUDPBoxContainer.Statistics_BadPackageCount / (mUDPBoxContainer.Statistics_TotalPackageCount + 0.00001f)));

            if (isMaster)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Box("Client:");
                var clientIPEndPointList = ClientIPConnectInfoList;
                for (int i = 0, iMax = clientIPEndPointList.Count; i < iMax; i++)
                {
                    var item = clientIPEndPointList[i];

                    if (item.Valid)
                        GUILayout.Box("[Client] Address: " + item.IPEndPoint.Address + " Port: " + item.IPEndPoint.Port + " Alive timer: " + item.AliveTimer + " Is Established: " + item.IsClientEstablished);
                }
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Box("Server:");
                if (MasterIPConnectInfo.Valid)
                {
                    var masterIPConnectInfo = mUDPBoxContainer.MasterIPConnectInfo;
                    GUILayout.Box("MasterIPEndPoint: " + masterIPConnectInfo.IPEndPoint + " Alive timer: " + masterIPConnectInfo.AliveTimer + " Is Established: " + masterIPConnectInfo.IsClientEstablished);
                }
                GUILayout.EndVertical();
            }
        }
    }
}
