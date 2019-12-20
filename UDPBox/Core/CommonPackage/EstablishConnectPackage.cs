using System;
using System.Collections.Generic;
using System.Text;

namespace Hont.UDPBoxPackage
{
    public class EstablishConnectPackage : Package
    {
        public enum ESenderType { Server, Client }

        public ESenderType SenderType { get; set; }
        public string IpAddress { get; set; }
        public int BeginPort { get; set; }
        public int EndPort { get; set; }
        public bool IsReceipt { get; set; }


        public EstablishConnectPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.Type = (int)EPackageType.System;
            base.ID = UDPBoxUtility.ESTABLISH_CONNECT_ID;

            Args = new PackageArgument[]
                 {
                    new PackageArgument_Int(),
                    new PackageArgument_String(),
                    new PackageArgument_Int(),
                    new PackageArgument_Int(),
                    new PackageArgument_Bool(),
                 };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_Int).Value = (int)SenderType;
            (Args[1] as PackageArgument_String).Value = IpAddress;
            (Args[2] as PackageArgument_Int).Value = BeginPort;
            (Args[3] as PackageArgument_Int).Value = EndPort;
            (Args[4] as PackageArgument_Bool).Value = IsReceipt;

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            if (!result) return false;

            SenderType = (ESenderType)(Args[0] as PackageArgument_Int).Value;
            IpAddress = (Args[1] as PackageArgument_String).Value;
            BeginPort = (Args[2] as PackageArgument_Int).Value;
            EndPort = (Args[3] as PackageArgument_Int).Value;
            IsReceipt = (Args[4] as PackageArgument_Bool).Value;

            return result;
        }
    }
}
