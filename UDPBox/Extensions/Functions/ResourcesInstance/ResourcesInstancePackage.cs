using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class ResourcesInstancePackage : Package
    {
        public enum EOperate { PushToServer, ApplyEffect }
        public EOperate Op { get; set; }
        public string ResourcesPath { get; set; }
        public Vector3 InstancedPosition { get; set; }
        public Vector3 InstancedRotation { get; set; }


        public ResourcesInstancePackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = UDPBoxExtensionConsts.RESOURCES_INSTANCE;
            base.Type = (short)EPackageType.Need_Ack_Session;

            Args = new PackageArgument[]
            {
                new PackageArgument_Byte(),
                new PackageArgument_String(),
                new PackageArgument_Vector3(),
                new PackageArgument_Vector3(),
            };
        }

        public override byte[] Serialize()
        {
            var arg_byte = Args[0] as PackageArgument_Byte;
            var arg_string = Args[1] as PackageArgument_String;
            var arg_position = Args[2] as PackageArgument_Vector3;
            var arg_rotation = Args[3] as PackageArgument_Vector3;

            arg_byte.Value = (byte)Op;
            arg_string.Value = ResourcesPath;
            arg_position.Value = InstancedPosition;
            arg_rotation.Value = InstancedRotation;

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            if (!result) return result;

            Op = (EOperate)(Args[0] as PackageArgument_Byte).Value;
            ResourcesPath = (Args[1] as PackageArgument_String).Value;
            InstancedPosition = (Args[2] as PackageArgument_Vector3).Value;
            InstancedRotation = (Args[3] as PackageArgument_Vector3).Value;

            return result;
        }
    }
}
