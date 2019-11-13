﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class SyncTransforms_Syncer : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBox;
        public float delay = 13f / 1000f;
        SyncTransformsPackage mPackageTemplate;


        IEnumerator Start()
        {
            mPackageTemplate = new SyncTransformsPackage(udpBox.PackageHeadBytes);
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

                        mPackageTemplate.Op = SyncTransformsPackage.EOperate.Fetch;
                        udpBox.UDPBox.SendMessage(mPackageTemplate.Serialize(), client.IPEndPoint);
                    }
                }
                else
                {
                    mPackageTemplate.Op = SyncTransformsPackage.EOperate.FetchAll;
                    udpBox.UDPBox.SendMessage(mPackageTemplate.Serialize(), udpBox.MasterIPConnectInfo);
                }

                yield return waitForSecond;
            }
        }
    }
}
