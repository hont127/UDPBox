using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;

namespace Hont.UDPBoxPackage
{
    public class UDPBox_Test : MonoBehaviour
    {
        UDPBoxBroadcast mUDPBoxBroadcast;
        UDPBox mUDPBoxConnect;


        void OnEnable()
        {
            var udpClient = UDPBoxUtility.GeterateUdpClient(1235);

            mUDPBoxConnect = new UDPBox(udpClient, UDPBoxUtility.DefaultHead);
            mUDPBoxConnect.Start();
            mUDPBoxBroadcast = new UDPBoxBroadcast(udpClient, 1234);
        }

        void OnDisable()
        {
            mUDPBoxConnect.Dispose();
            mUDPBoxBroadcast.ReleaseThread();
        }

        void OnGUI()
        {
            if (GUILayout.Button("Sent"))
            {
                var package = new PingPongPackage(UDPBoxUtility.DefaultHeadBytes);
                mUDPBoxBroadcast.StartBroadcast(package.Serialize());
            }
        }
    }
}
