using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hont.UDPBoxPackage
{
    class PackageArgument_UShort : PackageArgument
    {
        public ushort Value { get; set; }


        public override void Deserialize(BinaryReader binaryReader)
        {
            Value = binaryReader.ReadUInt16();
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Value);
        }
    }
}
