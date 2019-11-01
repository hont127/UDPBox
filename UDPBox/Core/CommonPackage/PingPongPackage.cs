using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hont.UDPBoxPackage
{
    public class PingPongPackage : Package
    {
        public enum EPingPong { Ping, Pong }
        public EPingPong PingPong { get; set; }


        public PingPongPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = UDPBoxUtility.PING_PONG_ID;

            Args = new PackageArgument[] { new PackageArgument_Int() };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_Int).Value = (int)PingPong;

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            PingPong = (EPingPong)(base.Args[0] as PackageArgument_Int).Value;

            return result;
        }
    }
}
