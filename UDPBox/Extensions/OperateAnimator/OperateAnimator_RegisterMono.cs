using Hont.UDPBoxPackage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    [DefaultExecutionOrder(5)]
    public class OperateAnimator_RegisterMono : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;
        public List<OperateAnimator_AnimatorMono> animatorMonoList;
        OperateAnimatorHandler mHandler;


        public void OperateAnimatorBroadcast(OperateAnimatorData[] datas)
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

            mHandler = new OperateAnimatorHandler(udpBoxContainer.GetNativeContainer());
            mHandler.animatorMonoList = animatorMonoList;

            udpBoxContainer.UDPBox.RegistHandler(mHandler);
        }
    }
}
