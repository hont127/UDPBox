using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class OperateAnimatorHandler : HandlerBase
    {
        UDPBoxContainer mUdpBoxContainer;
        public List<OperateAnimator_AnimatorMono> animatorMonoList;
        OperateAnimatorPackage mTemplate;


        public OperateAnimatorHandler(UDPBoxContainer container)
        {
            mUdpBoxContainer = container;
            mTemplate = new OperateAnimatorPackage(container.PackageHeadBytes);
            animatorMonoList = new List<OperateAnimator_AnimatorMono>(32);
        }

        public void SendUDPBoxBroadcastMessage(OperateAnimatorData[] datas)
        {
            if (mUdpBoxContainer.State == UDPBoxContainer.EState.NoClients || mUdpBoxContainer.State == UDPBoxContainer.EState.NoServer) return;

            if (mUdpBoxContainer.IsMaster)
            {
                mTemplate.Op = OperateAnimatorPackage.EOperate.ApplyEffect;
                mTemplate.DataList.Clear();
                mTemplate.DataList.AddRange(datas);

                UnityEngine.Debug.Log("mTemplate.DataList: " + mTemplate.DataList.Count);
                foreach (var item in mTemplate.DataList)
                {
                    UnityEngine.Debug.Log("item.op: " + item.Op);
                    UnityEngine.Debug.Log("item.stateName: " + item.StateName);
                }

                DispatchToClients(mTemplate.Serialize());
            }
            else
            {
                mTemplate.Op = OperateAnimatorPackage.EOperate.PushToServer;
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
                case OperateAnimatorPackage.EOperate.PushToServer:
                    mTemplate.Op = OperateAnimatorPackage.EOperate.ApplyEffect;
                    ApplyEffect();
                    DispatchToClients(mTemplate.Serialize(), ipEndPoint);
                    break;
                case OperateAnimatorPackage.EOperate.ApplyEffect:
                    ApplyEffect();
                    break;
                default:
                    break;
            }
        }

        protected override short[] GetCacheProcessableID()
        {
            return new short[] { UDPBoxExtensionConsts.OPERATE_ANIMATOR };
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

            UnityEngine.Debug.Log("dataList.Count: " + dataList.Count);

            for (int i = 0, iMax = dataList.Count; i < iMax; i++)
            {
                var item = dataList[i];

                var animator_component = animatorMonoList.Find(m => m.networkID == item.NetworkID).animator;

                switch (item.Op)
                {
                    case EOperateAnimator_InternalOperate.PlayState:

                        UDPBox_GameThreadMediator.Instance.EnqueueToUpdateQueue(() =>
                        {
                            animator_component.Play(item.StateName, 0, 0);
                        });

                        break;
                    case EOperateAnimator_InternalOperate.SetFloat:

                        UDPBox_GameThreadMediator.Instance.EnqueueToUpdateQueue(() =>
                        {
                            animator_component.SetFloat(item.VariableName, item.VariableValue_Float);
                        });

                        break;
                    case EOperateAnimator_InternalOperate.SetBoolean:

                        UDPBox_GameThreadMediator.Instance.EnqueueToUpdateQueue(() =>
                        {
                            animator_component.SetBool(item.VariableName, item.VariableValue_Bool);
                        });

                        break;
                    case EOperateAnimator_InternalOperate.SetTrigger:

                        UDPBox_GameThreadMediator.Instance.EnqueueToUpdateQueue(() =>
                        {
                            animator_component.SetTrigger(item.VariableName);
                        });

                        break;
                    case EOperateAnimator_InternalOperate.SetInteger:

                        UDPBox_GameThreadMediator.Instance.EnqueueToUpdateQueue(() =>
                        {
                            animator_component.SetInteger(item.VariableName, item.VariableValue_Int);
                        });

                        break;
                    default:
                        break;
                }
            }
        }
    }
}
