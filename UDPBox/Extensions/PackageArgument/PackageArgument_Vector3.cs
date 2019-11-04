using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Hont.UDPBoxPackage
{
    public class PackageArgument_Vector3 : PackageArgument
    {
        public Vector3 Value { get; set; }


        public override void Deserialize(BinaryReader binaryReader)
        {
            var vec = new Vector3();
            vec.x = binaryReader.ReadSingle();
            vec.y = binaryReader.ReadSingle();
            vec.z = binaryReader.ReadSingle();
            Value = vec;
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Value.x);
            binaryWriter.Write(Value.y);
            binaryWriter.Write(Value.z);
        }
    }
}
