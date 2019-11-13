using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class OperateGOTransformPackage : Package
    {
        public enum EOperate { PushToServer, ApplyEffect }
        public EOperate Op { get; set; }
        public List<OperateGOTransformData> DataList { get; set; }


        public OperateGOTransformPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = UDPBoxExtensionConsts.OPERATE_GO_TRANSFORM;
            base.Type = (short)EPackageType.Need_Ack_Session;

            DataList = new List<OperateGOTransformData>(8);

            Args = new PackageArgument[]
            {
                new PackageArgument_IntList(),
                new PackageArgument_IntList(),
                new PackageArgument_BoolList(),
                new PackageArgument_Vector3List(),
                new PackageArgument_Vector3List(),
                new PackageArgument_Vector3List(),
            };
        }

        public override byte[] Serialize()
        {
            var network_id_list = (Args[0] as PackageArgument_IntList).Value;
            var op_list = (Args[1] as PackageArgument_IntList).Value;
            var arg_active_state_list = (Args[2] as PackageArgument_BoolList).Value;
            var arg_position_list = (Args[3] as PackageArgument_Vector3List).Value;
            var arg_rotation_list = (Args[4] as PackageArgument_Vector3List).Value;
            var arg_localScale_list = (Args[5] as PackageArgument_Vector3List).Value;

            network_id_list.Clear();
            op_list.Clear();
            arg_active_state_list.Clear();
            arg_position_list.Clear();
            arg_rotation_list.Clear();
            arg_localScale_list.Clear();

            for (int i = 0, iMax = DataList.Count; i < iMax; i++)
            {
                var item = DataList[i];

                network_id_list.Add(item.NetworkID);
                op_list.Add((int)item.Op);
                arg_active_state_list.Add(item.ActiveState);
                arg_position_list.Add(item.Position);
                arg_rotation_list.Add(item.Rotation);
                arg_localScale_list.Add(item.LocalScale);
            }

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            if (!result) return result;

            var network_id_list = (Args[0] as PackageArgument_IntList).Value;
            var op_list = (Args[1] as PackageArgument_IntList).Value;
            var arg_active_state_list = (Args[2] as PackageArgument_BoolList).Value;
            var arg_position_list = (Args[3] as PackageArgument_Vector3List).Value;
            var arg_rotation_list = (Args[4] as PackageArgument_Vector3List).Value;
            var arg_localScale_list = (Args[5] as PackageArgument_Vector3List).Value;

            DataList.Clear();
            for (int i = 0, iMax = network_id_list.Count; i < iMax; i++)
            {
                var networkID = network_id_list[i];
                var op = op_list[i];
                var active_state = arg_active_state_list[i];
                var position = arg_position_list[i];
                var rotation = arg_rotation_list[i];
                var localScale = arg_localScale_list[i];

                DataList.Add(new OperateGOTransformData()
                {
                    NetworkID = networkID,
                    Op = (EOperateGOTransform_InternalOperate)op,
                    ActiveState = active_state,
                    Position = position,
                    Rotation = rotation,
                    LocalScale = localScale,
                });
            }

            return result;
        }
    }
}
