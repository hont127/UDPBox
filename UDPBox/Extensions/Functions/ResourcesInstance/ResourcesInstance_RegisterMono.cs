using Hont.UDPBoxPackage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    [DefaultExecutionOrder(5)]
    public class ResourcesInstance_RegisterMono : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;
        ResourcesInstanceHandler mHandler;


        public void InstanceResourceBroadcast(string resourcesPath, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
        {
            mHandler.SendUDPBoxBroadcastMessage(resourcesPath, position, rotation);
        }

        void OnEnable()
        {
            StartCoroutine(WaitAndInitialization());
        }

        void OnDisable()
        {
            udpBoxContainer.UDPBox.UnregistHandler(mHandler);
        }

        IEnumerator WaitAndInitialization()
        {
            yield return new WaitUntil(() => udpBoxContainer.State != UDPBoxContainer.EState.NoStart);

            mHandler = new ResourcesInstanceHandler(udpBoxContainer.GetNativeContainer(), udpBoxContainer.PackageHeadBytes);

            udpBoxContainer.UDPBox.RegistHandler(mHandler);
        }
    }
}
