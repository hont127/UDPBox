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
        public int udpBoxBeginPort = 1240;
        public int udpBoxEndPort = 1250;
        [Tooltip("format: \"192.168.1.\"")]
        public string broadcastNetprefixIP = "192.168.1.";
        public string proj_prefix = "Demo";
        public int sendMsgThreadSleepTime_MS = 35;

        public UDPBox UDPBox { get { return mUDPBoxContainer.UDPBox; } }
        public UDPBoxContainer.EState State { get { return mUDPBoxContainer.State; } }

        public byte[] PackageHeadBytes { get { return mUDPBoxContainer.PackageHeadBytes; } }

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


        public void SendUDPMessageWithRandomPort(byte[] bytes, UDPBoxContainer.ConnectInfo connectInfo)
        {
            mUDPBoxContainer.SendUDPMessageWithRandomPort(bytes, connectInfo);
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
            mUDPBoxContainer.UnregistHandler(handler);
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
            mUDPBoxContainer.UdpBoxBeginPort = udpBoxBeginPort;
            mUDPBoxContainer.UdpBoxEndPort = udpBoxEndPort;
            mUDPBoxContainer.BroadcastNetPrefixIP = broadcastNetprefixIP;

            mUDPBoxContainer.Initialization(proj_prefix + "_" + UDPBoxUtility.DefaultHead);
            mUDPBoxContainer.SendMsgThreadSleepTime = sendMsgThreadSleepTime_MS;

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

        public int GetRandomUDPBoxPort()
        {
            return mUDPBoxContainer.GetRandomUDPBoxPort();
        }

        public UDPBoxContainer.ConnectInfo UpdateConnectInfoToRandomPort(UDPBoxContainer.ConnectInfo connectInfo)
        {
            return mUDPBoxContainer.UpdateConnectInfoToRandomPort(connectInfo);
        }

        void OnEnable()
        {
            UDPBox_GameThreadMediator.Instance.GetHashCode();

            mUDPBoxContainer = new UDPBoxContainer();
            mUDPBoxContainer.OnException += (exception) =>
            {
                UnityEngine.Debug.LogError(exception);
            };

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
            GUILayout.Box("Bad Package Rate: " + (mUDPBoxContainer.Statistics_BadPackageCount / (mUDPBoxContainer.Statistics_TotalPackageCount + 0.00001f)).ToString("F3"));

            if (isMaster)
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Box("Client:");
                var clientIPEndPointList = ClientIPConnectInfoList;
                for (int i = 0, iMax = clientIPEndPointList.Count; i < iMax; i++)
                {
                    var item = clientIPEndPointList[i];

                    if (item.Valid)
                        GUILayout.Box("[Client] Address: " + item.IPEndPoint.Address + " Port: " + item.IPEndPoint.Port + " Alive timer: " + item.AliveTimer.ToString("F3") + " Is Established: " + item.IsClientEstablished);
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
                    GUILayout.Box("MasterIPEndPoint: " + masterIPConnectInfo.IPEndPoint + " Alive timer: " + masterIPConnectInfo.AliveTimer.ToString("F3") + " Is Established: " + masterIPConnectInfo.IsClientEstablished);
                }
                GUILayout.EndVertical();
            }
        }
    }
}
