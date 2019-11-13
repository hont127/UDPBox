using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class LevelStateChanged_BroadcastClients : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;
        public int newStateID;
        public string argInfo;


        void OnEnable()
        {
            var levelStateChangePackage = new LevelStateChangedPackage(udpBoxContainer.PackageHeadBytes);
            levelStateChangePackage.StateID = newStateID;
            levelStateChangePackage.ArgInfo = argInfo;

            var bytes = levelStateChangePackage.Serialize();

            foreach (var clientInfo in udpBoxContainer.ClientIPConnectInfoList)
            {
                udpBoxContainer.SendUDPMessage(bytes, clientInfo.IPEndPoint);
            }
        }
    }
}
