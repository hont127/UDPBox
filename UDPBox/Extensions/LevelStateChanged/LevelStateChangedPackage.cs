using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class LevelStateChangedPackage : Package
    {
        public int StateID;
        public string ArgInfo;
        public override bool EnabledCompress => true;


        public LevelStateChangedPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = UDPBoxExtensionConsts.LEVEL_STATE_CHANGED;
            base.Type = (short)EPackageType.Need_Ack_Session;

            Args = new PackageArgument[]
            {
                new PackageArgument_Int(),
                new PackageArgument_String(),
            };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_Int).Value = StateID;
            (Args[1] as PackageArgument_String).Value = ArgInfo;

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            if (!result) return result;

            StateID = (Args[0] as PackageArgument_Int).Value;
            ArgInfo = (Args[1] as PackageArgument_String).Value;

            return result;
        }
    }
}
