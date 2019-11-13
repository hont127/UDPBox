using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public class OperateAnimator_Test : MonoBehaviour
    {
        public OperateAnimator_RegisterMono handler;
        public int networkID;
        public EOperateAnimator_InternalOperate op;
        public string stateName;
        public string variableName;
        public int variableValue_Int;
        public bool variableValue_Bool;
        public float variableValue_Float;


        void OnEnable()
        {
            handler.OperateAnimatorBroadcast(new OperateAnimatorData[]{
                new OperateAnimatorData()
                {
                    Op = op
                    , NetworkID = networkID
                    , StateName = stateName
                    , VariableName = variableName
                    , VariableValue_Int = variableValue_Int
                    , VariableValue_Bool = variableValue_Bool
                    , VariableValue_Float = variableValue_Float
                }
                });
        }
    }
}
