using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class RTT_Test_Demo_Script : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;
        RTT_TestPackage mRTT_TestPackage;


        void Awake()
        {
            mRTT_TestPackage = new RTT_TestPackage(udpBoxContainer.PackageHeadBytes);
        }

        void OnGUI()
        {
            GUILayout.Space(80f);

            if (GUILayout.Button("RTT Test Start"))
            {
                mRTT_TestPackage.Op = RTT_TestPackage.EOp.A;
                mRTT_TestPackage.ATime = System.DateTime.Now.Ticks;
                UnityEngine.Debug.Log("udpBoxContainer.MasterIPConnectInfo.IPEndPoint: " + udpBoxContainer.MasterIPConnectInfo.IPEndPoint);
                udpBoxContainer.SendUDPMessage(mRTT_TestPackage.Serialize(), udpBoxContainer.MasterIPConnectInfo.IPEndPoint);
            }
        }
    }
}
