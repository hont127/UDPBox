using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    class PackageArgument_Vector4List : PackageArgument
    {
        public List<Vector4> Value { get; set; }


        public PackageArgument_Vector4List()
        {
            Value = new List<Vector4>(16);
        }

        public override void Deserialize(BinaryReader binaryReader)
        {
            var valueLength = binaryReader.ReadInt32();

            Value.Clear();
            for (int i = 0, iMax = valueLength; i < iMax; i++)
            {
                var x = binaryReader.ReadSingle();
                var y = binaryReader.ReadSingle();
                var z = binaryReader.ReadSingle();
                var w = binaryReader.ReadSingle();
                Value.Add(new Vector4(x, y, z, w));
            }
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Value.Count);

            for (int i = 0, iMax = Value.Count; i < iMax; i++)
            {
                var item = Value[i];
                binaryWriter.Write(item.x);
                binaryWriter.Write(item.y);
                binaryWriter.Write(item.z);
                binaryWriter.Write(item.w);
            }
        }
    }
}
