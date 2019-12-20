using Hont.UDPBoxPackage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    [DefaultExecutionOrder(5)]
    public class GetMasterClientCount_RegisterMono : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;
        public bool isDebug;

        GetMasterClientCountHandler mHandler;


        void OnEnable()
        {
            StartCoroutine(WaitAndInitialization());
        }

        void OnDisable()
        {
            udpBoxContainer.UDPBox.UnregistHandler(mHandler);
        }

        void OnGUI()
        {
            if (!isDebug) return;

            GUILayout.Box("Client Count: " + mHandler.ClientCount);
        }

        IEnumerator WaitAndInitialization()
        {
            yield return new WaitUntil(() => udpBoxContainer.State != UDPBoxContainer.EState.NoStart);

            mHandler = new GetMasterClientCountHandler(udpBoxContainer.GetNativeContainer());
            udpBoxContainer.UDPBox.RegistHandler(mHandler);
        }
    }
}
