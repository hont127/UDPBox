using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class GetMasterClientCountPackage : Package
    {
        public enum EOperate { Get, Set }
        public EOperate Op { get; set; }
        public int ClientCount { get; set; }


        public GetMasterClientCountPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = UDPBoxExtensionConsts.GET_MASTER_CLIENT_COUNT;

            Args = new PackageArgument[]
            {
                new PackageArgument_Int(),
                new PackageArgument_Int()
            };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_Int).Value = (int)Op;
            (Args[1] as PackageArgument_Int).Value = ClientCount;

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            Op = (EOperate)(base.Args[0] as PackageArgument_Int).Value;
            ClientCount = (base.Args[1] as PackageArgument_Int).Value;

            return result;
        }
    }
}
