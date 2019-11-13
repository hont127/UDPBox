using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public struct OperateGOTransformData
    {
        public int NetworkID { get; set; }
        public EOperateGOTransform_InternalOperate Op { get; set; }
        public bool ActiveState { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 LocalScale { get; set; }
    }
}
