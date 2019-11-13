using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class PackageArgument_Vector2 : PackageArgument
    {
        public Vector2 Value { get; set; }


        public override void Deserialize(BinaryReader binaryReader)
        {
            var vec = new Vector2();
            vec.x = binaryReader.ReadSingle();
            vec.y = binaryReader.ReadSingle();
            Value = vec;
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Value.x);
            binaryWriter.Write(Value.y);
        }
    }
}
