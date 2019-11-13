using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class PackageArgument_AnimatorList : PackageArgument
    {
        public struct AnimatorInfo
        {
            public int ID { get; set; }
            public EOperateAnimator_InternalOperate Op { get; set; }
            public string StateName { get; set; }
            public string VariableName { get; set; }
            public float VariableValue_Float { get; set; }
            public int VariableValue_Int { get; set; }
            public bool VariableValue_Bool { get; set; }
        }

        public List<AnimatorInfo> Value { get; set; }


        public PackageArgument_AnimatorList(int capacity = 16)
        {
            Value = new List<AnimatorInfo>(capacity);
        }

        public override void Deserialize(BinaryReader binaryReader)
        {
            var transformInfoArrayLength = binaryReader.ReadInt32();
            Value.Clear();
            for (int i = 0; i < transformInfoArrayLength; i++)
            {
                var animatorInfo = new AnimatorInfo();
                animatorInfo.ID = binaryReader.ReadInt32();
                animatorInfo.Op = (EOperateAnimator_InternalOperate)binaryReader.ReadByte();
                animatorInfo.StateName = binaryReader.ReadString();
                animatorInfo.VariableName = binaryReader.ReadString();
                animatorInfo.VariableValue_Int = binaryReader.ReadInt32();
                animatorInfo.VariableValue_Float = binaryReader.ReadSingle();
                animatorInfo.VariableValue_Bool = binaryReader.ReadBoolean();
                Value.Add(animatorInfo);
            }
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Value.Count);
            for (int i = 0, iMax = Value.Count; i < iMax; i++)
            {
                var transformInfo = Value[i];

                if (transformInfo.StateName == null)
                    transformInfo.StateName = "";

                if (transformInfo.VariableName == null)
                    transformInfo.VariableName = "";

                binaryWriter.Write(transformInfo.ID);
                binaryWriter.Write((byte)transformInfo.Op);
                binaryWriter.Write(transformInfo.StateName);
                binaryWriter.Write(transformInfo.VariableName);
                binaryWriter.Write(transformInfo.VariableValue_Bool);
                binaryWriter.Write(transformInfo.VariableValue_Float);
                binaryWriter.Write(transformInfo.VariableValue_Int);
            }
        }
    }
}
