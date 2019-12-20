using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hont.UDPBoxPackage;
using System.Net;

namespace Hont.UDPBoxExtensions
{
    public class RequestString_Test_Script : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpboxContainer;
        public UDPBoxPureContainer_Mono udpBoxPureContainer;
        public RequestString_RegisterMono requestString;
        RequestStringPackage mTestPackage;


        void Awake()
        {
            mTestPackage = new RequestStringPackage(udpboxContainer == null
                ? udpBoxPureContainer.UDPBox.PackageHeadBytes : udpboxContainer.PackageHeadBytes);

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

                if (udpBoxPureContainer != null)
                {
                    udpBoxPureContainer.UDPBox.SendMessage(mTestPackage.Serialize()
                        , new IPEndPoint(IPAddress.Parse("127.0.0.1"), udpBoxPureContainer.udpBoxBeginPort));
                }
                else
                {
                    udpboxContainer.UDPBox.SendMessage(mTestPackage.Serialize()
                        , new IPEndPoint(IPAddress.Parse("127.0.0.1"), udpboxContainer.GetRandomUDPBoxPort()));
                }
            }
        }
    }
}
