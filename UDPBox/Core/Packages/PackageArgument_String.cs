using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hont.UDPBoxPackage
{
    class PackageArgument_String : PackageArgument
    {
        public string Value { get; set; }


        public override void Deserialize(BinaryReader binaryReader)
        {
            Value = binaryReader.ReadString();
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Value);
        }
    }
}
