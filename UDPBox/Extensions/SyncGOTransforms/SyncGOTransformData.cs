using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public struct SyncGOTransformData
    {
        public int NetworkID { get; set; }

        public bool Active { get; set; }
        public float Pos_X { get; set; }
        public float Pos_Y { get; set; }
        public float Pos_Z { get; set; }
        public float Euler_X { get; set; }
        public float Euler_Y { get; set; }
        public float Euler_Z { get; set; }
        public float SCALE_X { get; set; }
        public float SCALE_Y { get; set; }
        public float SCALE_Z { get; set; }
    }
}
