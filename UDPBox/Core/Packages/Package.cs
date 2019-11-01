using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hont.UDPBoxPackage
{
    public class Package : ICloneable
    {
        public byte[] HeadBytes { get; private set; }
        public short Type { get; set; }
        public ushort MagicNumber { get; set; }
        public short ID { get; set; }
        public uint ContentLength { get; set; }
        public PackageArgument[] Args { get; set; }
        public virtual bool EnabledCompress { get { return false; } }


        public Package(byte[] headBytes)
        {
            HeadBytes = headBytes;
        }

        public virtual byte[] Serialize()
        {
            ++MagicNumber;

            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);

            binaryWriter.Write(HeadBytes);
            binaryWriter.Write(Type);
            binaryWriter.Write(MagicNumber);
            binaryWriter.Write(ID);
            binaryWriter.Flush();
            var length1 = (int)binaryWriter.BaseStream.Length;
            binaryWriter.Write(0);//占位符

            if (Args != null)
            {
                binaryWriter.Write(Args.Length);

                if (EnabledCompress)
                {
                    var memoryStream_compress = new MemoryStream();
                    var binaryWriter_compress = new BinaryWriter(memoryStream_compress);

                    for (int i = 0, iMax = Args.Length; i < iMax; i++)
                    {
                        var arg = Args[i];
                        arg.Serialize(binaryWriter_compress);
                    }

                    binaryWriter_compress.Flush();
                    var bytes = memoryStream_compress.ToArray();
                    bytes = CompressHelper.ZipBytesCompress(bytes);
                    binaryWriter.Write(bytes.Length);
                    binaryWriter.Write(bytes);
                }
                else
                {
                    for (int i = 0, iMax = Args.Length; i < iMax; i++)
                    {
                        var arg = Args[i];
                        arg.Serialize(binaryWriter);
                    }
                }
            }
            else
            {
                binaryWriter.Write(0);
            }

            binaryWriter.Flush();
            var length2 = (uint)binaryWriter.BaseStream.Length;
            ContentLength = length2 - (uint)length1 - 4;

            binaryWriter.Seek(length1, SeekOrigin.Begin);
            binaryWriter.Write(ContentLength);

            var result = memoryStream.ToArray();
            memoryStream.Close();
            memoryStream.Dispose();
            binaryWriter.Close();
            binaryWriter.Dispose();

            return result;
        }

        public virtual bool Deserialize(byte[] bytes)
        {
            if (!UDPBoxUtility.CheckByteHead(bytes, HeadBytes)) return false;

            var memoryStream = new MemoryStream(bytes);
            var binaryReader = new BinaryReader(memoryStream);

            binaryReader.ReadBytes(HeadBytes.Length);
            Type = binaryReader.ReadInt16();
            MagicNumber = binaryReader.ReadUInt16();
            ID = binaryReader.ReadInt16();
            ContentLength = binaryReader.ReadUInt32();

            var argLength = binaryReader.ReadInt32();

            if (Args == null)
                throw new System.NotSupportedException("需要预先把参数创建出来才可反序列化！应从模板类中进行处理！");

            if (EnabledCompress)
            {
                var bytesLength_Compress = binaryReader.ReadInt32();
                var compressed_bytes = binaryReader.ReadBytes(bytesLength_Compress);
                var uncompress_bytes = CompressHelper.ZipBytesDecompress(compressed_bytes);
                var memoryStream_uncompress = new MemoryStream(uncompress_bytes);
                var binaryReader_uncompress = new BinaryReader(memoryStream_uncompress);

                for (int i = 0, iMax = argLength; i < iMax; i++)
                {
                    Args[i].Deserialize(binaryReader_uncompress);
                }
            }
            else
            {
                for (int i = 0, iMax = argLength; i < iMax; i++)
                {
                    Args[i].Deserialize(binaryReader);
                }
            }

            return true;
        }

        public object Clone()
        {
            return base.MemberwiseClone();
        }
    }
}
