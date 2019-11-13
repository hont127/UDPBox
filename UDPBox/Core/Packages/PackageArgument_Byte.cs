using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Hont.UDPBoxPackage
{
    class PackageArgument_Byte : PackageArgument
    {
        public byte Value { get; set; }


        public override void Deserialize(BinaryReader binaryReader)
        {
            Value = binaryReader.ReadByte();
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Value);
        }
    }
}
