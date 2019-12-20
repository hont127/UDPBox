using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxPackage
{
    public class ACK_Test : MonoBehaviour
    {
        public UDPBox mUDPBox1;
        public UDPBox mUDPBox2;


        void OnEnable()
        {
            var udpClients1 = UDPBoxFactory.GeterateUdpClientsArray(1234, 1235);
            mUDPBox1 = new UDPBox(udpClients1, UDPBoxUtility.DefaultHead);
            var udpClients2 = UDPBoxFactory.GeterateUdpClientsArray(1235, 1236);
            mUDPBox2 = new UDPBox(udpClients2, UDPBoxUtility.DefaultHead);
        }

        void OnDisable()
        {
            mUDPBox1.Dispose();
            mUDPBox2.Dispose();
        }

        void OnGUI()
        {
            if (mUDPBox1 == null) return;
            if (mUDPBox2 == null) return;

            if (GUILayout.Button("Start UDPBox1"))
                mUDPBox1.Start();

            if (GUILayout.Button("Start UDPBox2"))
                mUDPBox2.Start();

            if (GUILayout.Button("udpbox2 -> udpbox1"))
            {
                var test = new ACKTestPackage(mUDPBox2.PackageHeadBytes);
                var bytes = test.Serialize();
                mUDPBox2.SendMessage(bytes, new System.Net.IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234));
            }
        }
    }
}
