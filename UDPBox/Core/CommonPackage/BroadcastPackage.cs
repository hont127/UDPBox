using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hont.UDPBoxPackage
{
    public class BroadcastPackage : Package
    {
        public string ProjectPrefix { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }


        public BroadcastPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.Type = (int)EPackageType.System;
            base.ID = UDPBoxUtility.BROADCAST_PACKAGE_ID;

            Args = new PackageArgument[3]
                 {
                    new PackageArgument_String(),
                    new PackageArgument_String(),
                    new PackageArgument_Int(),
                 };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_String).Value = ProjectPrefix;
            (Args[1] as PackageArgument_String).Value = IpAddress;
            (Args[2] as PackageArgument_Int).Value = Port;

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            if (!result) return false;

            ProjectPrefix = (Args[0] as PackageArgument_String).Value;
            IpAddress = (Args[1] as PackageArgument_String).Value;
            Port = (Args[2] as PackageArgument_Int).Value;

            return result;
        }
    }
}
