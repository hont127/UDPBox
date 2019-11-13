using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public class OperateGOTransform_Test : MonoBehaviour
    {
        public OperateGOTransform_RegisterMono handler;
        public int networkID;
        public EOperateGOTransform_InternalOperate op;
        public bool activeState;


        void OnEnable()
        {
            handler.OperateGOTransformBroadcast(new OperateGOTransformData[]{
                new OperateGOTransformData()
                {
                    Op = op
                    , NetworkID = networkID
                    , ActiveState = activeState
                    , Position = transform.position
                    , Rotation = transform.rotation.eulerAngles
                    , LocalScale = transform.localScale
                }
            });
        }
    }
}
