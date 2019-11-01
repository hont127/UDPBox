using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hont.UDPBoxPackage
{
    public class PackageArgument_UnityBase : PackageArgument
    {
        public int NetworkID { get; set; }


        public override void Deserialize(BinaryReader binaryReader)
        {
            NetworkID = binaryReader.ReadInt32();
        }

        public override void Serialize(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(NetworkID);
        }
    }
}
