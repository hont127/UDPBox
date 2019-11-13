using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Hont.UDPBoxPackage
{
    class PackageArgument_Float : PackageArgument
    {
        public float Value { get; set; }


        public override void Deserialize(BinaryReader binaryReader)
        {
            Value = binaryReader.ReadSingle();
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Value);
        }
    }
}
