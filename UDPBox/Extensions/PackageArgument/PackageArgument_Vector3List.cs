using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    class PackageArgument_Vector3List : PackageArgument
    {
        public List<Vector3> Value { get; set; }


        public PackageArgument_Vector3List()
        {
            Value = new List<Vector3>(16);
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
                Value.Add(new Vector3(x, y, z));
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
            }
        }
    }
}
