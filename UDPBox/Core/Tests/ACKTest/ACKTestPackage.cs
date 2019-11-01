using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hont.UDPBoxPackage
{
    public class ACKTestPackage : Package
    {
        public ACKTestPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = 1234;
            base.Type = (short)EPackageType.Need_Ack_Session;
        }
    }
}
