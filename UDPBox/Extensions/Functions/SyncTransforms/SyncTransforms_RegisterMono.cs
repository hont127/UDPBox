using Hont.UDPBoxPackage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    [DefaultExecutionOrder(5)]
    public class SyncTransforms_RegisterMono : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;
        public List<SyncTransforms_TransformMono> transformsList;

        SyncTransformsHandler mHandler;


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

            mHandler = new SyncTransformsHandler(udpBoxContainer.PackageHeadBytes) { transformsList = transformsList };

            udpBoxContainer.UDPBox.RegistHandler(mHandler);
        }
    }
}
