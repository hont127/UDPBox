using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Hont.UDPBoxPackage
{
    class PackageArgument_BoolList : PackageArgument
    {
        public List<bool> Value { get; set; }


        public PackageArgument_BoolList()
        {
            Value = new List<bool>(16);
        }

        public override void Deserialize(BinaryReader binaryReader)
        {
            var valueLength = binaryReader.ReadInt32();

            Value.Clear();
            for (int i = 0, iMax = valueLength; i < iMax; i++)
            {
                Value.Add(binaryReader.ReadBoolean());
            }
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Value.Count);

            for (int i = 0, iMax = Value.Count; i < iMax; i++)
            {
                binaryWriter.Write(Value[i]);
            }
        }
    }
}
