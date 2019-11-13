using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Hont.UDPBoxPackage
{
    class PackageArgument_Short : PackageArgument
    {
        public short Value { get; set; }


        public override void Deserialize(BinaryReader binaryReader)
        {
            Value = binaryReader.ReadInt16();
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Value);
        }
    }
}
