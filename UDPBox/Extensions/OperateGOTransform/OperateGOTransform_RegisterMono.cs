using Hont.UDPBoxPackage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    [DefaultExecutionOrder(5)]
    public class OperateGOTransform_RegisterMono : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;
        public List<OperateGOTransform_TransformMono> goTransformMonoList;
        OperateGOTransformHandler mHandler;


        public void OperateGOTransformBroadcast(OperateGOTransformData[] datas)
        {
            mHandler.SendUDPBoxBroadcastMessage(datas);
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

            mHandler = new OperateGOTransformHandler(udpBoxContainer.GetNativeContainer());
            mHandler.goTransformMonoList = goTransformMonoList;

            udpBoxContainer.UDPBox.RegistHandler(mHandler);
        }
    }
}
