using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class SyncGOTransformsHandler : HandlerBase
    {
        public List<SyncGOTransforms_TransformMono> transformsList;
        SyncGOTransformsPackage mTemplate;


        public SyncGOTransformsHandler()
        {
            transformsList = new List<SyncGOTransforms_TransformMono>(8);
            mTemplate = new SyncGOTransformsPackage(UDPBoxUtility.DefaultHeadBytes);
        }

        protected override short[] GetCacheProcessableID()
        {
            return new short[] { UDPBoxExtensionConsts.SYNC_GO_TRANSFORMS };
        }

        public override void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint)
        {
            mTemplate.Deserialize(packageBytes);

            switch (mTemplate.Op)
            {
                case SyncGOTransformsPackage.EOperate.Push://接收到推送消息的处理
                    {
                        var transformList = mTemplate.TransformList;
                        UpdateTransforms(transformList);
                    }
                    break;

                case SyncGOTransformsPackage.EOperate.Fetch://有主机向该机器拉取自己控制的部分数据
                    {
                        FetchSelfTransforms(transformsList, udpBox, ipEndPoint);
                    }
                    break;

                case SyncGOTransformsPackage.EOperate.FetchAll://有主机向该机器拉取全部数据
                    {
                        FetchSelfTransforms(transformsList, udpBox, ipEndPoint, true);
                    }
                    break;
            }
        }

        void UpdateTransforms(List<SyncGOTransformData> transformList)
        {
            for (int i = 0, iMax = transformList.Count; i < iMax; i++)
            {
                var transformInfo = transformList[i];

                UDPBox_GameThreadMediator.Instance?.EnqueueToUpdateQueue(() =>
                {
                    var transformMono = transformsList.Find(m => m.networkID == transformInfo.NetworkID);
                    if (transformMono != null)
                    {
                        transformMono.gameObject.SetActive(transformInfo.Active);
                        transformMono.dstPosition = new UnityEngine.Vector3(transformInfo.Pos_X, transformInfo.Pos_Y, transformInfo.Pos_Z);
                        transformMono.dstEulerAngle = new UnityEngine.Vector3(transformInfo.Euler_X, transformInfo.Euler_Y, transformInfo.Euler_Z);
                        transformMono.dstLocalScale = new UnityEngine.Vector3(transformInfo.SCALE_X, transformInfo.SCALE_Y, transformInfo.SCALE_Z);
                    }
                });
            }
        }

        void FetchSelfTransforms(List<SyncGOTransforms_TransformMono> transformsList, UDPBox udpBox, IPEndPoint ipEndPoint, bool isFetchAll = false)
        {
            UDPBox_GameThreadMediator.Instance?.EnqueueToUpdateQueue(() =>
            {
                var package = new SyncGOTransformsPackage(UDPBoxUtility.DefaultHeadBytes);
                package.Op = SyncGOTransformsPackage.EOperate.Push;

                for (int i = 0, iMax = transformsList.Count; i < iMax; i++)
                {
                    var transformMono = transformsList[i];
                    if (!transformMono.isSelfControl && !isFetchAll) continue;

                    var transformInfo = new SyncGOTransformData();

                    transformInfo.NetworkID = transformMono.networkID;
                    transformInfo.Active = transformMono.gameObject.activeSelf;
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

                udpBox.SendMessage(package.Serialize(), ipEndPoint);
            });
        }
    }
}
