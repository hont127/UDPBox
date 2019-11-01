using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class RTT_TestPackage : Package
    {
        public enum EOp { A, B }
        public long ATime { get; set; }
        public long BTime { get; set; }
        public EOp Op { get; set; }


        public RTT_TestPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = UDPBoxExtensionConsts.RTT;

            Args = new PackageArgument[]
            {
                new PackageArgument_Long(),
                new PackageArgument_Long(),
                new PackageArgument_Long(),
                new PackageArgument_Int(),
            };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_Long).Value = ATime;
            (Args[1] as PackageArgument_Long).Value = BTime;
            (Args[3] as PackageArgument_Int).Value = (int)Op;

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);

            ATime = (Args[0] as PackageArgument_Long).Value;
            BTime = (Args[1] as PackageArgument_Long).Value;
            Op = (EOp)(Args[3] as PackageArgument_Int).Value;

            return result;
        }
    }
}
