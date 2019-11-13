using System;
using System.Collections.Generic;
using System.Text;

namespace Hont.UDPBoxPackage
{
    public class EstablishServerConnectPackage : Package
    {
        public string IpAddress { get; set; }
        public int BeginPort { get; set; }
        public int EndPort { get; set; }


        public EstablishServerConnectPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.Type = (int)EPackageType.System;
            base.ID = UDPBoxUtility.ESTABLISH_SERVER_CONNECT_ID;

            Args = new PackageArgument[3]
                 {
                    new PackageArgument_String(),
                    new PackageArgument_Int(),
                    new PackageArgument_Int(),
                 };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_String).Value = IpAddress;
            (Args[1] as PackageArgument_Int).Value = BeginPort;
            (Args[2] as PackageArgument_Int).Value = EndPort;

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            if (!result) return false;

            IpAddress = (Args[0] as PackageArgument_String).Value;
            BeginPort = (Args[1] as PackageArgument_Int).Value;
            EndPort = (Args[2] as PackageArgument_Int).Value;

            return result;
        }
    }
}
