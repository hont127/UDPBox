using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public struct OperateAnimatorData
    {
        public int NetworkID { get; set; }

        public EOperateAnimator_InternalOperate Op { get; set; }

        public string StateName { get; set; }
        public string VariableName { get; set; }
        public float VariableValue_Float { get; set; }
        public int VariableValue_Int { get; set; }
        public bool VariableValue_Bool { get; set; }
    }
}
