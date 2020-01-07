using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Hont.UDPBoxPackage
{
    public class LargePackage : Package
    {
        public short ConcretePackageID { get; set; }
        public long ConcretePackageUUID { get; set; }
        public int SegmentID { get; set; }
        public int SegmentTotalCount { get; set; }
        public List<byte> BytesList { get; set; }


        public LargePackage(byte[] headBytes)
            : base(headBytes)
        {
            base.Type = (int)EPackageType.System;
            base.ID = UDPBoxUtility.LARGE_PACKAGE_ID;

            Args = new PackageArgument[]
                 {
                    new PackageArgument_Short(),
                    new PackageArgument_Long(),
                    new PackageArgument_Int(),
                    new PackageArgument_Int(),
                    new PackageArgument_ByteList(),
                 };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_Short).Value = ConcretePackageID;
            (Args[1] as PackageArgument_Long).Value = ConcretePackageUUID;
            (Args[2] as PackageArgument_Int).Value = SegmentID;
            (Args[3] as PackageArgument_Int).Value = SegmentTotalCount;
            (Args[4] as PackageArgument_ByteList).Value = BytesList;

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            if (!result) return false;

            ConcretePackageID = (Args[0] as PackageArgument_Short).Value;
            ConcretePackageUUID = (Args[1] as PackageArgument_Long).Value;
            SegmentID = (Args[2] as PackageArgument_Int).Value;
            SegmentTotalCount = (Args[3] as PackageArgument_Int).Value;
            BytesList = (Args[4] as PackageArgument_ByteList).Value;

            return result;
        }
    }
}
