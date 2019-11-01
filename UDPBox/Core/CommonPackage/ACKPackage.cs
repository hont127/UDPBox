using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hont.UDPBoxPackage
{
    public class ACKPackage : Package
    {
        public ushort ACK_MagicNumber { get; set; }
        public short ACK_ID { get; set; }


        public ACKPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = UDPBoxUtility.ACK_ID;
            base.Type = (short)EPackageType.System;

            base.Args = new PackageArgument[2]
              {
                    new PackageArgument_UShort(),
                    new PackageArgument_Short(),
              };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_UShort).Value = ACK_MagicNumber;
            (Args[1] as PackageArgument_Short).Value = ACK_ID;

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            if (!result) return false;

            ACK_MagicNumber = (Args[0] as PackageArgument_UShort).Value;
            ACK_ID = (Args[1] as PackageArgument_Short).Value;

            return result;
        }
    }
}
