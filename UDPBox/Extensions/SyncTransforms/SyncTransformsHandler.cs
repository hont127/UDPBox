using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class SyncTransformsHandler : HandlerBase
    {
        public List<SyncTransforms_TransformMono> transformsList;
        SyncTransformsPackage mTemplate;


        public SyncTransformsHandler(byte[] packageHeadBytes)
        {
            transformsList = new List<SyncTransforms_TransformMono>(8);
            mTemplate = new SyncTransformsPackage(packageHeadBytes);
        }

        protected override short[] GetCacheProcessableID()
        {
            return new short[] { UDPBoxExtensionConsts.SYNC_TRANSFORMS };
        }

        public override void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint)
        {
            mTemplate.Deserialize(packageBytes);

            switch (mTemplate.Op)
            {
                case SyncTransformsPackage.EOperate.Push:
                    {
                        var transformList = mTemplate.TransformList;
                        UpdateTransforms(transformList);
                    }
                    break;

                case SyncTransformsPackage.EOperate.Fetch:
                    {
                        FetchSelfTransforms(transformsList, udpBox, ipEndPoint);
                    }
                    break;

                case SyncTransformsPackage.EOperate.FetchAll:
                    {
                        FetchSelfTransforms(transformsList, udpBox, ipEndPoint, true);
                    }
                    break;

                case SyncTransformsPackage.EOperate.RestoreList:
                    {
                        var transformList = mTemplate.TransformList;
                        UpdateTransforms(transformList);
                    }
                    break;
            }
        }

        void UpdateTransforms(List<SyncTransformData> transformList)
        {
            for (int i = 0, iMax = transformList.Count; i < iMax; i++)
            {
                var transformInfo = transformList[i];

                UDPBox_GameThreadMediator.Instance?.EnqueueToUpdateQueue(() =>
                {
                    var transformMono = transformsList.Find(m => m.networkID == transformInfo.NetworkID);
                    if (transformMono != null)
                    {
                        transformMono.transform.position = new UnityEngine.Vector3(transformInfo.Pos_X, transformInfo.Pos_Y, transformInfo.Pos_Z);
                        transformMono.transform.eulerAngles = new UnityEngine.Vector3(transformInfo.Euler_X, transformInfo.Euler_Y, transformInfo.Euler_Z);
                        transformMono.transform.localScale = new UnityEngine.Vector3(transformInfo.SCALE_X, transformInfo.SCALE_Y, transformInfo.SCALE_Z);
                    }
                });
            }
        }

        void FetchSelfTransforms(List<SyncTransforms_TransformMono> transformsList, UDPBox udpBox, IPEndPoint ipEndPoint, bool isFetchAll = false)
        {
            UDPBox_GameThreadMediator.Instance?.EnqueueToUpdateQueue(() =>
            {
                var package = new SyncTransformsPackage(mTemplate.HeadBytes);
                package.Op = SyncTransformsPackage.EOperate.Push;

                for (int i = 0, iMax = transformsList.Count; i < iMax; i++)
                {
                    var transformMono = transformsList[i];
                    if (!transformMono.isSelfControl && !isFetchAll) continue;

                    var transformInfo = new SyncTransformData();

                    transformInfo.NetworkID = transformMono.networkID;
                    transformInfo.Pos_X = transformMono.transform.position.x;
                    transformInfo.Pos_Y = transformMono.transform.position.y;
                    transformInfo.Pos_Z = transformMono.transform.position.z;
                    transformInfo.Euler_X = transformMono.transform.eulerAngles.x;
                    transformInfo.Euler_Y = transformMono.transform.eulerAngles.y;
                    transformInfo.Euler_Z = transformMono.transform.eulerAngles.z;
                    transformInfo.SCALE_X = transformMono.transform.localScale.x;
                    transformInfo.SCALE_Y = transformMono.transform.localScale.y;
                    transformInfo.SCALE_Z = transformMono.transform.localScale.z;

                    package.TransformList.Add(transformInfo);
                }

                var transformPackageBytes = package.Serialize();
                udpBox.SendMessage(transformPackageBytes, ipEndPoint);
            });
        }
    }
}
