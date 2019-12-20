using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class SyncTransforms_Syncer : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;
        public float delay = 13f / 1000f;
        SyncTransformsPackage mPackageTemplate;
        UDPBoxContainer mUdpBoxContainer;


        public void SetUDPBoxContainer(UDPBoxContainer container)
        {
            mUdpBoxContainer = container;
        }

        IEnumerator Start()
        {
            if (udpBoxContainer != null)
                SetUDPBoxContainer(udpBoxContainer.GetNativeContainer());

            mPackageTemplate = new SyncTransformsPackage(mUdpBoxContainer.PackageHeadBytes);
            var waitForSecond = new WaitForSeconds(delay);

            while (true)
            {
                if (mUdpBoxContainer.State != UDPBoxContainer.EState.HasClients && mUdpBoxContainer.State != UDPBoxContainer.EState.HasServer)
                {
                    yield return null;
                    continue;
                }

                if (mUdpBoxContainer.IsMaster)
                {
                    mPackageTemplate.TransformList.Clear();
                    for (int i = 0, iMax = mUdpBoxContainer.ClientIPConnectList.Count; i < iMax; i++)
                    {
                        var client = mUdpBoxContainer.ClientIPConnectList[i];

                        mPackageTemplate.Op = SyncTransformsPackage.EOperate.Fetch;
                        mUdpBoxContainer.SendUDPMessageToRandomPort(mPackageTemplate.Serialize(), client);
                        Debug.Log("Send to client!");
                    }
                }
                else
                {
                    mPackageTemplate.Op = SyncTransformsPackage.EOperate.FetchAll;
                    mUdpBoxContainer.SendUDPMessageToRandomPort(mPackageTemplate.Serialize(), mUdpBoxContainer.MasterIPConnectInfo);
                    Debug.Log("Send to master!");
                }

                yield return waitForSecond;
            }
        }
    }
}
