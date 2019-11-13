using Hont.UDPBoxPackage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    [DefaultExecutionOrder(5)]
    public class RTT_Test_RegisterMono : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;

        RTT_TestHandler mHandler;


        void OnEnable()
        {
            mHandler = new RTT_TestHandler(udpBoxContainer.PackageHeadBytes);

            udpBoxContainer.UDPBox.RegistHandler(mHandler);
        }

        void OnDisable()
        {
            udpBoxContainer.UDPBox.UnregistHandler(mHandler);
        }
    }
}
