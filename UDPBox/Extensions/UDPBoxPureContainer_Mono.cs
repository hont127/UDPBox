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
    public class UDPBoxPureContainer_Mono : MonoBehaviour
    {
        public bool isDebug;

        public int udpBoxBeginPort = 1236;
        public int udpBoxEndPort = 1237;
        public int sendMsgThreadSleepTime_MS = 35;
        public int recvMsgThreadSleepTime_MS = 32;

        UDPBox mUDPBox;
        public UDPBox UDPBox { get { return mUDPBox; } }


        public void RegistHandler(HandlerBase handler)
        {
            mUDPBox.RegistHandler(handler);
        }

        public void UnregistHandler(HandlerBase handler)
        {
            mUDPBox.UnregistHandler(handler);
        }

        public void RegistWorkThreadOperate(Action operateAction)
        {
            mUDPBox.RegistWorkThreadOperate(operateAction);
        }

        public void UnregistWorkThreadOperate(Action operateAction)
        {
            mUDPBox.UnregistWorkThreadOperate(operateAction);
        }

        void OnEnable()
        {
            mUDPBox = new UDPBox();
        }

        void Start()
        {
            UDPBox_GameThreadMediator.InitializationInUnityGameThread();

            var udpClientsArray = UDPBoxFactory.GeterateUdpClientsArray(udpBoxBeginPort, udpBoxEndPort);
            mUDPBox.Initialization(udpClientsArray, UDPBoxUtility.DefaultHead);
            mUDPBox.Start();
        }

        void OnDestroy()
        {
            mUDPBox.Dispose();
        }

        void OnGUI()
        {
            if (!isDebug) return;


        }
    }
}
