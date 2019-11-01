using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public static class UDPBoxExtensionMethods
    {
        public static void SendMessage(this UDPBox udpBox, byte[] bytes, UDPBoxContainer.ConnectInfo connectInfo)
        {
            udpBox.SendMessage(bytes, connectInfo.IPEndPoint);
        }
    }
}

