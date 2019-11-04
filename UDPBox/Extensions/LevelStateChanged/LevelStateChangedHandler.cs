using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class LevelStateChangedHandler : HandlerBase
    {
        LevelStateChangedPackage mTemplate;

        /// <summary>
        /// arg1 - state id, arg2 - arg info.
        /// </summary>
        public event Action<int, string> OnStateChanged;


        public LevelStateChangedHandler()
        {
            mTemplate = new LevelStateChangedPackage(UDPBoxUtility.DefaultHeadBytes);
        }

        protected override short[] GetCacheProcessableID()
        {
            return new short[] { UDPBoxExtensionConsts.LEVEL_STATE_CHANGED };
        }

        public override void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint)
        {
            mTemplate.Deserialize(packageBytes);

            var stateID = mTemplate.StateID;
            var argInfo = mTemplate.ArgInfo;
            UDPBox_GameThreadMediator.Instance.EnqueueToUpdateQueue(() =>
            {
                OnStateChanged?.Invoke(stateID, argInfo);
            });
        }
    }
}
