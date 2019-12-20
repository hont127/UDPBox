using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hont.UDPBoxExtensions;

namespace Hont.UDPBoxPackage
{
    public partial class UDPBoxFactory
    {
        public static UDPBoxBroadcast GenerateStandardUDPBoxBroadcastAndSetup(
            UDPBox_UDPClient udpClient
            , int broadcastSendPort
            , string netPrefixIP
            , UDPBoxContainer container)
        {
            var broadcast = new UDPBoxBroadcast(udpClient, broadcastSendPort);
            var broadcastPackage = new BroadcastPackage(container.PackageHeadBytes);

            if (container.IsMaster)
            {
                broadcast.ListenBroadcast((bytes, endPoint) =>
                {
                    if (UDPBoxUtility.PackageIsBroken(bytes, container.PackageHeadBytes)) return;
                    if (!UDPBoxUtility.ComparePackageID(bytes, container.PackageHeadBytes, broadcastPackage.ID)) return;

                    if (!broadcastPackage.Deserialize(bytes)) return;

                    var ipEndPoint = new IPEndPoint(IPAddress.Parse(broadcastPackage.IpAddress), broadcastPackage.BeginPort);
                    if (ipEndPoint.Address.Equals(container.SelfIPAddress)
                        && ipEndPoint.Port < container.UdpBoxEndPort && ipEndPoint.Port >= container.UdpBoxBeginPort) return;
                    //Avoid self connect to the self.

                    var establishConnectPackage = new EstablishConnectPackage(container.PackageHeadBytes);
                    establishConnectPackage.SenderType = EstablishConnectPackage.ESenderType.Server;
                    establishConnectPackage.IpAddress = container.SelfIPAddress.ToString();
                    establishConnectPackage.BeginPort = container.UdpBoxBeginPort;
                    establishConnectPackage.EndPort = container.UdpBoxEndPort;
                    establishConnectPackage.IsReceipt = false;
                    container.SendUDPMessage(establishConnectPackage.Serialize(), ipEndPoint);
                    //Server notify to client.
                });
            }
            else
            {
                broadcastPackage.IpAddress = container.SelfIPAddress.ToString();
                broadcastPackage.BeginPort = container.UdpBoxBeginPort;
                broadcastPackage.EndPort = container.UdpBoxEndPort;

                broadcast.StartBroadcast(broadcastPackage.Serialize(), netPrefixIP);
                //Client through broadcast notify to server.
            }

            return broadcast;
        }

        public static UDPBox_UDPClient GeterateUdpClient(int port)
        {
            var mUdpClient = new UDPBox_UDPClient(port);
            mUdpClient.Client.SendTimeout = 1000;
            mUdpClient.Client.ReceiveTimeout = 10000;

            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            mUdpClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
            //屏蔽目标机器没有接收逻辑时报错。

            return mUdpClient;
        }

        public static UDPBox_UDPClient[] GeterateUdpClientsArray(int beginPort, int endPort)
        {
            var result = new UDPBox_UDPClient[endPort - beginPort];

            for (int i = 0; i < result.Length; i++)
                result[i] = GeterateUdpClient(beginPort + i);

            return result;
        }
    }
}
