using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    class GetMasterClientCountHandler : HandlerBase
    {
        public int ClientCount { get; set; }

        GetMasterClientCountPackage mTemplate;
        UDPBoxContainer mUdpBoxContainer;


        public GetMasterClientCountHandler(UDPBoxContainer udpBoxContainer)
        {
            mUdpBoxContainer = udpBoxContainer;
            mTemplate = new GetMasterClientCountPackage(UDPBoxUtility.DefaultHeadBytes);
        }

        protected override short[] GetCacheProcessableID()
        {
            return new short[] { UDPBoxExtensionConsts.GET_MASTER_CLIENT_COUNT };
        }

        public override void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint)
        {
            mTemplate.Deserialize(packageBytes);

            switch (mTemplate.Op)
            {
                case GetMasterClientCountPackage.EOperate.Get:
                    mTemplate.ClientCount = mUdpBoxContainer.ClientIPConnectList.Count;
                    mTemplate.Op = GetMasterClientCountPackage.EOperate.Set;
                    udpBox.SendMessage(mTemplate.Serialize(), ipEndPoint);
                    break;
                case GetMasterClientCountPackage.EOperate.Set:
                    ClientCount = mTemplate.ClientCount;
                    break;
                default:
                    break;
            }


        }
    }
}
