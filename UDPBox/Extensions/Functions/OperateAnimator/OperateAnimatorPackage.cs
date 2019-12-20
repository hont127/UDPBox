using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class OperateAnimatorPackage : Package
    {
        public enum EOperate { PushToServer, ApplyEffect }
        public EOperate Op { get; set; }
        public List<OperateAnimatorData> DataList { get; set; }


        public OperateAnimatorPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = UDPBoxExtensionConsts.OPERATE_ANIMATOR;
            base.Type = (short)EPackageType.Need_Ack_Session;

            DataList = new List<OperateAnimatorData>(8);

            Args = new PackageArgument[]
            {
                new PackageArgument_AnimatorList(),
            };
        }

        public override byte[] Serialize()
        {
            var arg_animator_list = Args[0] as PackageArgument_AnimatorList;

            var animatorList = arg_animator_list.Value;
            animatorList.Clear();
            for (int i = 0, iMax = DataList.Count; i < iMax; i++)
            {
                var item = DataList[i];

                animatorList.Add(new PackageArgument_AnimatorList.AnimatorInfo()
                {
                    ID = item.NetworkID,
                    Op = item.Op,
                    StateName = item.StateName,
                    VariableName = item.VariableName,
                    VariableValue_Bool = item.VariableValue_Bool,
                    VariableValue_Float = item.VariableValue_Float,
                    VariableValue_Int = item.VariableValue_Int,
                });
            }

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            if (!result) return result;

            DataList.Clear();

            var arg_animator_list = Args[0] as PackageArgument_AnimatorList;
            var animatorList = arg_animator_list.Value;
            for (int i = 0, iMax = animatorList.Count; i < iMax; i++)
            {
                var item = animatorList[i];

                DataList.Add(new OperateAnimatorData()
                {
                    NetworkID = item.ID,
                    Op = item.Op,
                    StateName = item.StateName,
                    VariableName = item.VariableName,
                    VariableValue_Bool = item.VariableValue_Bool,
                    VariableValue_Float = item.VariableValue_Float,
                    VariableValue_Int = item.VariableValue_Int,
                });
            }

            return result;
        }
    }
}
