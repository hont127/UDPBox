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
        public static UDPBoxContainer GenerateUDPBoxContainerInUnityGameThread(bool excepteEventBind = true, bool setHardDiskLogger = false)
        {
            UDPBox_GameThreadMediator.InitializationInUnityGameThread();
            var container = new UDPBoxContainer();

            if (excepteEventBind)
            {
                container.OnException += (exception) =>
                {
                    UnityEngine.Debug.LogError(exception);
                };
            }

            if (setHardDiskLogger)
            {
                container.SetLogger(new HardDiskUDPBoxLogger());
            }

            return container;
        }
    }
}
