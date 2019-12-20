using Hont.UDPBoxPackage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    [DefaultExecutionOrder(5)]
    public class SyncGOTransforms_RegisterMono : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;
        public List<SyncGOTransforms_TransformMono> transformsList;

        SyncGOTransformsHandler mHandler;


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

            mHandler = new SyncGOTransformsHandler(udpBoxContainer.PackageHeadBytes) { transformsList = transformsList };

            udpBoxContainer.UDPBox.RegistHandler(mHandler);
        }
    }
}
