using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxPackage
{
    public interface IUDPBoxLogger
    {
        void Log(string content, EUDPBoxLogType type);
    }
}
