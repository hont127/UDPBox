using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Hont.UDPBoxPackage;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public class RTT_TestHandler : HandlerBase
    {
        RTT_TestPackage mTemplate;


        public RTT_TestHandler(byte[] packageHeadBytes)
        {
            mTemplate = new RTT_TestPackage(packageHeadBytes);
        }

        protected override short[] GetCacheProcessableID()
        {
            return new short[] { UDPBoxExtensionConsts.RTT };
        }

        public override void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint)
        {
            mTemplate.Deserialize(packageBytes);

            UnityEngine.Debug.Log("mTemplate.Op: " + mTemplate.Op);

            switch (mTemplate.Op)
            {
                case RTT_TestPackage.EOp.A:

                    mTemplate.BTime = DateTime.Now.Ticks;
                    mTemplate.Op = RTT_TestPackage.EOp.B;
                    udpBox.SendMessage(mTemplate.Serialize(), ipEndPoint);

                    break;
                case RTT_TestPackage.EOp.B:
                    Debug.LogError("RTT: " + ((mTemplate.BTime - mTemplate.ATime) / (float)TimeSpan.TicksPerMillisecond) + " ms" + "   ipEndPoint: " + ipEndPoint);
                    break;
                default:
                    break;
            }
        }
    }
}
