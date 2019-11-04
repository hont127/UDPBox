using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Hont.UDPBoxPackage
{
    public class PackageArgument_Vector4 : PackageArgument
    {
        public Vector4 Value { get; set; }


        public override void Deserialize(BinaryReader binaryReader)
        {
            var vec = new Vector4();
            vec.x = binaryReader.ReadSingle();
            vec.y = binaryReader.ReadSingle();
            vec.z = binaryReader.ReadSingle();
            vec.w = binaryReader.ReadSingle();
            Value = vec;
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Value.x);
            binaryWriter.Write(Value.y);
            binaryWriter.Write(Value.z);
            binaryWriter.Write(Value.w);
        }
    }
}
