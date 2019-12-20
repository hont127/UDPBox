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
            var udpClients = UDPBoxFactory.GeterateUdpClientsArray(1235, 1236);

            mUDPBoxConnect = new UDPBox(udpClients, UDPBoxUtility.DefaultHead);
            mUDPBoxConnect.Start();
            mUDPBoxBroadcast = new UDPBoxBroadcast(udpClients[0], 1234);
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
