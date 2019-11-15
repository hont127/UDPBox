using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxPackage
{
    public sealed class EmptyUDPBoxLogger : IUDPBoxLogger
    {
        void IUDPBoxLogger.Log(string content, EUDPBoxLogType type)
        {
        }
    }
}
