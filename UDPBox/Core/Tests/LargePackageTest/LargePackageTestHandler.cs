using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Hont.UDPBoxExtensions;

namespace Hont.UDPBoxPackage
{
    public class LargePackageTestHandler : HandlerBase
    {
        LargePackageTestPackage mTemplate;


        public LargePackageTestHandler(byte[] packageHeadBytes)
        {
            mTemplate = new LargePackageTestPackage(packageHeadBytes);
        }

        public override void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint)
        {
            mTemplate.Deserialize(packageBytes);
            UnityEngine.Debug.Log("!! " + mTemplate.byteList.Count);
        }

        protected override short[] GetCacheProcessableID() { return new short[] { 1234 }; }
    }
}
