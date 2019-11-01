using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    [Serializable]
    public struct UserNameInfoData
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }

        public bool Valid { get { return IPAddress != null; } }
    }
}
