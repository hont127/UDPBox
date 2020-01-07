using Hont.UDPBoxPackage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    [DefaultExecutionOrder(5)]
    public class LargePackageTestHandler_RegisterMono : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;

        LargePackageTestHandler mHandler;


        void OnEnable()
        {
            mHandler = new LargePackageTestHandler(udpBoxContainer.PackageHeadBytes);

            udpBoxContainer.UDPBox.RegistHandler(mHandler);
        }

        void OnDisable()
        {
            udpBoxContainer.UDPBox.UnregistHandler(mHandler);
        }
    }
}
