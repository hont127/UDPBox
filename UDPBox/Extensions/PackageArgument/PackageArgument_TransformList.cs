using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hont.UDPBoxPackage
{
    public class PackageArgument_TransformList : PackageArgument
    {
        public struct TransformInfo
        {
            public int ID { get; set; }
            public float Pos_X { get; set; }
            public float Pos_Y { get; set; }
            public float Pos_Z { get; set; }
            public float Euler_X { get; set; }
            public float Euler_Y { get; set; }
            public float Euler_Z { get; set; }
            public float SCALE_X { get; set; }
            public float SCALE_Y { get; set; }
            public float SCALE_Z { get; set; }
        }

        public List<TransformInfo> Value { get; set; }


        public PackageArgument_TransformList(int capacity = 12)
        {
            Value = new List<TransformInfo>(capacity);
        }

        public override void Deserialize(BinaryReader binaryReader)
        {
            var transformInfoArrayLength = binaryReader.ReadInt32();
            Value.Clear();
            for (int i = 0; i < transformInfoArrayLength; i++)
            {
                var transformInfo = new TransformInfo();
                transformInfo.ID = binaryReader.ReadInt32();
                transformInfo.Pos_X = binaryReader.ReadSingle();
                transformInfo.Pos_Y = binaryReader.ReadSingle();
                transformInfo.Pos_Z = binaryReader.ReadSingle();
                transformInfo.Euler_X = binaryReader.ReadSingle();
                transformInfo.Euler_Y = binaryReader.ReadSingle();
                transformInfo.Euler_Z = binaryReader.ReadSingle();
                transformInfo.SCALE_X = binaryReader.ReadSingle();
                transformInfo.SCALE_Y = binaryReader.ReadSingle();
                transformInfo.SCALE_Z = binaryReader.ReadSingle();
                Value.Add(transformInfo);
            }
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Value.Count);
            for (int i = 0, iMax = Value.Count; i < iMax; i++)
            {
                var transformInfo = Value[i];

                binaryWriter.Write(transformInfo.ID);

                binaryWriter.Write(transformInfo.Pos_X);
                binaryWriter.Write(transformInfo.Pos_Y);
                binaryWriter.Write(transformInfo.Pos_Z);
                binaryWriter.Write(transformInfo.Euler_X);
                binaryWriter.Write(transformInfo.Euler_Y);
                binaryWriter.Write(transformInfo.Euler_Z);
                binaryWriter.Write(transformInfo.SCALE_X);
                binaryWriter.Write(transformInfo.SCALE_Y);
                binaryWriter.Write(transformInfo.SCALE_Z);
            }
        }
    }
}
