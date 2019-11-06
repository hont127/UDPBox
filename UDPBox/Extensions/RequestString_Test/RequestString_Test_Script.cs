using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hont.UDPBoxPackage;
using System.Net;

namespace Hont.UDPBoxExtensions
{
    public class RequestString_Test_Script : MonoBehaviour
    {
        public UDPBoxPureContainer_Mono udpBoxPureContainer;
        public RequestString_RegisterMono requestString;
        RequestStringPackage mTestPackage;


        void Awake()
        {
            mTestPackage = new RequestStringPackage(UDPBoxUtility.DefaultHeadBytes);

            requestString.OnProcessRequest = (str) =>
            {
                if (str == "hahaha")
                    return "hehehe";
                else
                    return "404";
            };

            requestString.OnProcessResponse = (req_cache, resp) =>
            {
                Debug.Log("req_cache: " + req_cache + " resp: " + resp);
            };
        }

        void OnGUI()
        {
            if (GUILayout.Button("Req"))
            {
                mTestPackage.Op = RequestStringPackage.EOp.Request;
                mTestPackage.Content = "hahaha";
                udpBoxPureContainer.UDPBox.SendMessage(mTestPackage.Serialize()
                    , new IPEndPoint(IPAddress.Parse("127.0.0.1"), udpBoxPureContainer.udpBoxPort));
            }
        }
    }
}
