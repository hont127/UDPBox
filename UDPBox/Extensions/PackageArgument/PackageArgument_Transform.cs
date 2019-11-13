using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class PackageArgument_Transform : PackageArgument_UnityBase
    {
        public float Pos_X { get; set; }
        public float Pos_Y { get; set; }
        public float Pos_Z { get; set; }
        public float Euler_X { get; set; }
        public float Euler_Y { get; set; }
        public float Euler_Z { get; set; }
        public float SCALE_X { get; set; }
        public float SCALE_Y { get; set; }
        public float SCALE_Z { get; set; }


        public override void Deserialize(BinaryReader binaryReader)
        {
            base.Deserialize(binaryReader);

            Pos_X = binaryReader.ReadSingle();
            Pos_Y = binaryReader.ReadSingle();
            Pos_Z = binaryReader.ReadSingle();

            Euler_X = binaryReader.ReadSingle();
            Euler_Y = binaryReader.ReadSingle();
            Euler_Z = binaryReader.ReadSingle();

            SCALE_X = binaryReader.ReadSingle();
            SCALE_Y = binaryReader.ReadSingle();
            SCALE_Z = binaryReader.ReadSingle();
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            base.Serialize(binaryWriter);

            binaryWriter.Write(Pos_X);
            binaryWriter.Write(Pos_Y);
            binaryWriter.Write(Pos_Z);

            binaryWriter.Write(Euler_X);
            binaryWriter.Write(Euler_Y);
            binaryWriter.Write(Euler_Z);

            binaryWriter.Write(SCALE_X);
            binaryWriter.Write(SCALE_Y);
            binaryWriter.Write(SCALE_Z);
        }
    }
}
