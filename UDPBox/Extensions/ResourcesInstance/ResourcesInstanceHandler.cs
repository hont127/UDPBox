using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class ResourcesInstanceHandler : HandlerBase
    {
        UDPBoxContainer mUdpBoxContainer;
        ResourcesInstancePackage mTemplate;


        public ResourcesInstanceHandler(UDPBoxContainer container)
        {
            mUdpBoxContainer = container;
            mTemplate = new ResourcesInstancePackage(UDPBoxUtility.DefaultHeadBytes);
        }

        public void SendUDPBoxBroadcastMessage(string resourcesPath, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
        {
            if (mUdpBoxContainer.IsMaster)
            {
                mTemplate.Op = ResourcesInstancePackage.EOperate.ApplyEffect;
                mTemplate.ResourcesPath = resourcesPath;
                mTemplate.InstancedPosition = position;
                mTemplate.InstancedRotation = rotation.eulerAngles;

                DispatchToClients(mTemplate.Serialize());
            }
            else
            {
                mTemplate.Op = ResourcesInstancePackage.EOperate.PushToServer;
                mTemplate.ResourcesPath = resourcesPath;
                mTemplate.InstancedPosition = position;
                mTemplate.InstancedRotation = rotation.eulerAngles;

                mUdpBoxContainer.SendUDPMessage(mTemplate.Serialize(), mUdpBoxContainer.MasterIPConnectInfo.IPEndPoint);
            }
        }

        public override void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint)
        {
            UnityEngine.Debug.Log("RECV ipEndPoint: " + ipEndPoint + " mTemplate.Op: " + mTemplate.Op);

            mTemplate.Deserialize(packageBytes);

            switch (mTemplate.Op)
            {
                case ResourcesInstancePackage.EOperate.PushToServer:
                    mTemplate.Op = ResourcesInstancePackage.EOperate.ApplyEffect;
                    ApplyEffect();
                    DispatchToClients(mTemplate.Serialize(), ipEndPoint);
                    break;
                case ResourcesInstancePackage.EOperate.ApplyEffect:
                    ApplyEffect();
                    break;
                default:
                    break;
            }
        }

        protected override short[] GetCacheProcessableID()
        {
            return new short[] { UDPBoxExtensionConsts.RESOURCES_INSTANCE };
        }

        void DispatchToClients(byte[] bytes, IPEndPoint ignoreIPEndPoint = null)
        {
            var clients = mUdpBoxContainer.ClientIPConnectList;
            for (int i = 0, iMax = clients.Count; i < iMax; i++)
            {
                var client = clients[i];

                if (!client.Valid) continue;
                if (ignoreIPEndPoint != null && client.IPEndPoint.Equals(ignoreIPEndPoint)) continue;

                mUdpBoxContainer.SendUDPMessage(bytes, client.IPEndPoint);
            }
        }

        void ApplyEffect()
        {
            var resourcesPath = mTemplate.ResourcesPath;
            var instancedPosition = mTemplate.InstancedPosition;
            var instancedRotation = mTemplate.InstancedRotation;

            UDPBox_GameThreadMediator.Instance.EnqueueToUpdateQueue(() =>
            {
                var go = UnityEngine.Object.Instantiate(UnityEngine.Resources.Load<UnityEngine.GameObject>(resourcesPath));
                go.transform.position = instancedPosition;
                go.transform.eulerAngles = instancedRotation;
            });
        }
    }
}
