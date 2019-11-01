using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hont.UDPBoxPackage
{
    public abstract class PackageArgument
    {
        public abstract void Serialize(BinaryWriter binaryWriter);
        public abstract void Deserialize(BinaryReader binaryReader);
    }
}
