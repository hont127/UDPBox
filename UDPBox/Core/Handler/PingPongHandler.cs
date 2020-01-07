using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Hont.UDPBoxPackage
{
    class PingPongHandler : HandlerBase
    {
        PingPongPackage mTemplate;


        public PingPongHandler(byte[] packageHead)
        {
            mTemplate = new PingPongPackage(packageHead);
        }

        protected override short[] GetCacheProcessableID()
        {
            return new short[] { UDPBoxUtility.PING_PONG_ID };
        }

        public override void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint)
        {
            if (!mTemplate.Deserialize(packageBytes)) return;

            mTemplate.PingPong = mTemplate.PingPong == PingPongPackage.EPingPong.Ping ? PingPongPackage.EPingPong.Pong : PingPongPackage.EPingPong.Ping;

            udpBox.SendMessage(mTemplate.Serialize(), ipEndPoint);
        }
    }
}
