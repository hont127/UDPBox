using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hont.UDPBoxPackage
{
    public class LargePackageTestPackage : Package
    {
        public List<byte> byteList;


        public LargePackageTestPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = 1234;
            base.Type = (short)EPackageType.Session;
            Args = new PackageArgument[] { new PackageArgument_ByteList() };

            byteList = new List<byte>();
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_ByteList).Value = byteList;

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            byteList = (base.Args[0] as PackageArgument_ByteList).Value;

            return result;
        }
    }
}
