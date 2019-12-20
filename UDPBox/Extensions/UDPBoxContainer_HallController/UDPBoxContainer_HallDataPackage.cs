using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class UDPBoxContainer_HallDataPackage : Package
    {
        public string IPAddress { get; set; }
        public int BeginPort { get; set; }
        public int EndPort { get; set; }
        public string RoomName { get; set; }


        public UDPBoxContainer_HallDataPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = UDPBoxExtensionConsts.HALL_INFO;

            Args = new PackageArgument[]
            {
                new PackageArgument_String(),
                new PackageArgument_Int(),
                new PackageArgument_Int(),
                new PackageArgument_String(),
            };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_String).Value = IPAddress;
            (Args[1] as PackageArgument_Int).Value = BeginPort;
            (Args[2] as PackageArgument_Int).Value = EndPort;
            (Args[3] as PackageArgument_String).Value = RoomName;

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            if (!result) return false;

            IPAddress = (Args[0] as PackageArgument_String).Value;
            BeginPort = (Args[1] as PackageArgument_Int).Value;
            EndPort = (Args[2] as PackageArgument_Int).Value;
            RoomName = (Args[3] as PackageArgument_String).Value;

            return result;
        }
    }
}
