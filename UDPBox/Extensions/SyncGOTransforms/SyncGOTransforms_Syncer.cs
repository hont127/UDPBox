using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class SyncGOTransforms_Syncer : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBox;
        public float delay = 13f / 1000f;
        SyncGOTransformsPackage mPackageTemplate;


        IEnumerator Start()
        {
            mPackageTemplate = new SyncGOTransformsPackage(UDPBoxUtility.DefaultHeadBytes);
            var waitForSecond = new WaitForSeconds(delay);

            while (true)
            {
                if (udpBox.State != UDPBoxContainer.EState.HasClients && udpBox.State != UDPBoxContainer.EState.HasServer)
                {
                    yield return null;
                    continue;
                }

                if (udpBox.isMaster)
                {
                    mPackageTemplate.TransformList.Clear();
                    for (int i = 0, iMax = udpBox.ClientIPConnectInfoList.Count; i < iMax; i++)
                    {
                        var client = udpBox.ClientIPConnectInfoList[i];

                        mPackageTemplate.Op = SyncGOTransformsPackage.EOperate.Fetch;
                        udpBox.UDPBox.SendMessage(mPackageTemplate.Serialize(), client.IPEndPoint);
                    }
                }
                else
                {
                    mPackageTemplate.Op = SyncGOTransformsPackage.EOperate.FetchAll;
                    udpBox.UDPBox.SendMessage(mPackageTemplate.Serialize(), udpBox.MasterIPConnectInfo);
                }

                yield return waitForSecond;
            }
        }
    }
}
