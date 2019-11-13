using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Hont.UDPBoxPackage;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public class OperateGOTransformHandler : HandlerBase
    {
        UDPBoxContainer mUdpBoxContainer;
        public List<OperateGOTransform_TransformMono> goTransformMonoList;
        OperateGOTransformPackage mTemplate;


        public OperateGOTransformHandler(UDPBoxContainer container)
        {
            mUdpBoxContainer = container;
            mTemplate = new OperateGOTransformPackage(container.PackageHeadBytes);
            goTransformMonoList = new List<OperateGOTransform_TransformMono>(32);
        }

        public void SendUDPBoxBroadcastMessage(OperateGOTransformData[] datas)
        {
            if (mUdpBoxContainer.State == UDPBoxContainer.EState.NoClients || mUdpBoxContainer.State == UDPBoxContainer.EState.NoServer) return;

            if (mUdpBoxContainer.IsMaster)
            {
                mTemplate.Op = OperateGOTransformPackage.EOperate.ApplyEffect;
                mTemplate.DataList.Clear();
                mTemplate.DataList.AddRange(datas);

                DispatchToClients(mTemplate.Serialize());
            }
            else
            {
                mTemplate.Op = OperateGOTransformPackage.EOperate.PushToServer;
                mTemplate.DataList.Clear();
                mTemplate.DataList.AddRange(datas);

                mUdpBoxContainer.SendUDPMessageWithRandomPort(mTemplate.Serialize(), mUdpBoxContainer.MasterIPConnectInfo);
            }
        }

        public override void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint)
        {
            mTemplate.Deserialize(packageBytes);

            switch (mTemplate.Op)
            {
                case OperateGOTransformPackage.EOperate.PushToServer:
                    mTemplate.Op = OperateGOTransformPackage.EOperate.ApplyEffect;
                    ApplyEffect();
                    DispatchToClients(mTemplate.Serialize(), ipEndPoint);
                    break;
                case OperateGOTransformPackage.EOperate.ApplyEffect:
                    ApplyEffect();
                    break;
                default:
                    break;
            }
        }

        protected override short[] GetCacheProcessableID()
        {
            return new short[] { UDPBoxExtensionConsts.OPERATE_GO_TRANSFORM };
        }

        void DispatchToClients(byte[] bytes, IPEndPoint ignoreIPEndPoint = null)
        {
            var clients = mUdpBoxContainer.ClientIPConnectList;
            for (int i = 0, iMax = clients.Count; i < iMax; i++)
            {
                var client = clients[i];

                if (!client.Valid) continue;
                if (ignoreIPEndPoint != null && client.IPEndPoint.Equals(ignoreIPEndPoint)) continue;

                mUdpBoxContainer.SendUDPMessageWithRandomPort(bytes, client);
            }
        }

        void ApplyEffect()
        {
            var dataList = mTemplate.DataList;

            for (int i = 0, iMax = dataList.Count; i < iMax; i++)
            {
                var item = dataList[i];

                var goTransform_component = goTransformMonoList.Find(m => m.networkID == item.NetworkID);

                switch (item.Op)
                {
                    case EOperateGOTransform_InternalOperate.SetActive:

                        UDPBox_GameThreadMediator.Instance.EnqueueToUpdateQueue(() =>
                        {
                            goTransform_component.gameObject.SetActive(item.ActiveState);
                        });

                        break;
                    case EOperateGOTransform_InternalOperate.SetPosition:

                        UDPBox_GameThreadMediator.Instance.EnqueueToUpdateQueue(() =>
                        {
                            goTransform_component.transform.position = item.Position;
                        });

                        break;
                    case EOperateGOTransform_InternalOperate.SetRotation:

                        UDPBox_GameThreadMediator.Instance.EnqueueToUpdateQueue(() =>
                        {
                            UnityEngine.Debug.Log(item.Rotation);
                            goTransform_component.transform.eulerAngles = item.Rotation;
                        });

                        break;
                    case EOperateGOTransform_InternalOperate.SetScale:

                        UDPBox_GameThreadMediator.Instance.EnqueueToUpdateQueue(() =>
                        {
                            goTransform_component.transform.localScale = item.LocalScale;
                        });

                        break;
                    default:
                        break;
                }
            }
        }
    }
}
