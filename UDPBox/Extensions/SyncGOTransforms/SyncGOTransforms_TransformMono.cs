using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public class SyncGOTransforms_TransformMono : MonoBehaviour
    {
        public int networkID;
        public bool isSelfControl;
        public float tween = 17f;
        public bool enableTween = true;

        [HideInInspector]
        public Vector3 dstPosition;
        [HideInInspector]
        public Vector3 dstEulerAngle;
        [HideInInspector]
        public Vector3 dstLocalScale;

        void LateUpdate()
        {
            if (isSelfControl) return;

            if (enableTween)
            {
                transform.position = Vector3.Lerp(transform.position, dstPosition, tween * Time.deltaTime);
                //transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, dstEulerAngle, tween * Time.deltaTime);
                transform.eulerAngles = dstEulerAngle;
                transform.localScale = Vector3.Lerp(transform.localScale, dstLocalScale, tween * Time.deltaTime);
            }
            else
            {
                transform.position = dstPosition;
                transform.eulerAngles = dstEulerAngle;
                transform.localScale = dstLocalScale;
            }
        }
    }
}
