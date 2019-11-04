using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public class SyncGOTransforms_TransformMonoMasterOverride : MonoBehaviour
    {
        [SerializeField]
        UDPBoxContainer_Mono udpBoxContainer;
        [SerializeField]
        SyncGOTransforms_TransformMono transformMono;


        void OnEnable()
        {
            transformMono.isSelfControl = udpBoxContainer.isMaster ? true : false;
        }
    }
}
