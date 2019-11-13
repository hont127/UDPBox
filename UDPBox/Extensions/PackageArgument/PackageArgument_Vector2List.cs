using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    class PackageArgument_Vector2List : PackageArgument
    {
        public List<Vector2> Value { get; set; }


        public PackageArgument_Vector2List()
        {
            Value = new List<Vector2>(16);
        }

        public override void Deserialize(BinaryReader binaryReader)
        {
            var valueLength = binaryReader.ReadInt32();

            Value.Clear();
            for (int i = 0, iMax = valueLength; i < iMax; i++)
            {
                var x = binaryReader.ReadSingle();
                var y = binaryReader.ReadSingle();
                Value.Add(new Vector2(x, y));
            }
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Value.Count);

            for (int i = 0, iMax = Value.Count; i < iMax; i++)
            {
                binaryWriter.Write(Value[i].x);
                binaryWriter.Write(Value[i].y);
            }
        }
    }
}
