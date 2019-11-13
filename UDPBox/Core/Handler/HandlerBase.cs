using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Hont.UDPBoxPackage
{
    public abstract class HandlerBase
    {
        short[] mCacheProcessableID;
        public short[] ProcessableID { get { return mCacheProcessableID ?? (mCacheProcessableID = GetCacheProcessableID()); } }


        public virtual void OnRegistedToUDPBox(UDPBox udpBox)
        {
        }

        public virtual void OnUnregistedFromUDPBox(UDPBox udpBox)
        {
        }

        protected abstract short[] GetCacheProcessableID();

        public abstract void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint);
    }
}
