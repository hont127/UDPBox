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
        public UDPBoxPureContainer_Mono udpBoxContainer;

        RequestStringHandler mHandler;

        public Func<string, string> OnProcessRequest;
        public Action<string, string> OnProcessResponse;


        void OnEnable()
        {
            mHandler = new RequestStringHandler(OnProcessRequestMethod, OnProcessResponseMethod);

            udpBoxContainer.UDPBox.RegistHandler(mHandler);
        }

        void OnDisable()
        {
            udpBoxContainer.UDPBox.UnregistHandler(mHandler);
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
