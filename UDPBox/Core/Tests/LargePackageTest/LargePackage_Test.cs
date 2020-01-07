using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hont.UDPBoxExtensions;

namespace Hont.UDPBoxPackage
{
    public class LargePackage_Test : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer_Mono;


        void OnEnable()
        {
        }

        void OnDisable()
        {
        }

        void OnGUI()
        {
            GUILayout.Space(200);

            if (GUILayout.Button("Send large package to master"))
            {
                var largeTestPackage = new LargePackageTestPackage(udpBoxContainer_Mono.PackageHeadBytes);
                largeTestPackage.byteList.AddRange(new byte[8 * 2]);
                var bytes = UDPBoxUtility.ConvertToLargePackageBytes(largeTestPackage, 8);
                for (int i = 0; i < bytes.Length; i++)
                {
                    var bytes_singleChunk = bytes[i];
                    //var id = UDPBoxUtility.GetPackageID(bytes_singleChunk, udpBoxContainer_Mono.PackageHeadBytes);
                    //Debug.Log("id: " + id);
                    udpBoxContainer_Mono.SendUDPMessageToRandomPort(bytes_singleChunk, udpBoxContainer_Mono.MasterIPConnectInfo);
                }
            }
        }
    }
}
