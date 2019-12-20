using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    [DefaultExecutionOrder(5)]
    public class RequestString_RegisterMono : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;
        public UDPBoxPureContainer_Mono udpBoxPureContainer;

        RequestStringHandler mHandler;

        public Func<string, string> OnProcessRequest;
        public Action<string, string> OnProcessResponse;


        void OnEnable()
        {
            mHandler = new RequestStringHandler(udpBoxContainer == null
                    ? udpBoxPureContainer.UDPBox.PackageHeadBytes : udpBoxContainer.PackageHeadBytes
                , OnProcessRequestMethod, OnProcessResponseMethod);

            if (udpBoxPureContainer != null)
                udpBoxPureContainer.UDPBox.RegistHandler(mHandler);
            else
                udpBoxContainer.RegistHandler(mHandler);
        }

        void OnDisable()
        {
            if (udpBoxPureContainer != null)
                udpBoxPureContainer.UDPBox.UnregistHandler(mHandler);
            else
                udpBoxContainer.UnregistHandler(mHandler);
        }

        string OnProcessRequestMethod(string request)
        {
            return OnProcessRequest(request);
        }

        void OnProcessResponseMethod(string requestCache, string response)
        {
            OnProcessResponse(requestCache, response);
        }
    }
}
