using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class UserNameListHandler : HandlerBase
    {
        public const float REFRESH_DELAY = 1f;

        UDPBoxContainer mUDPBoxContainer;

        UserNameListPackage mTemplate;
        long mLastWorkThreadTime;
        float mRefreshDelayTimer;
        public string SelfUserName { get; set; }
        public List<UserNameInfoData> UserNameInfoList { get; private set; }


        public UserNameListHandler(byte[] packageHeadBytes, UDPBoxContainer container)
        {
            mUDPBoxContainer = container;
            mTemplate = new UserNameListPackage(packageHeadBytes);

            UserNameInfoList = new List<UserNameInfoData>(16);
        }

        public override void OnRegistedToUDPBox(UDPBox udpBox)
        {
            base.OnRegistedToUDPBox(udpBox);

            udpBox.RegistWorkThreadOperate(WorkThreadOperateLoop);
        }

        public override void OnUnregistedFromUDPBox(UDPBox udpBox)
        {
            base.OnUnregistedFromUDPBox(udpBox);

            udpBox.UnregistWorkThreadOperate(WorkThreadOperateLoop);
        }

        protected override short[] GetCacheProcessableID()
        {
            return new short[] { UDPBoxExtensionConsts.USER_NAME_LIST };
        }

        public override void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint)
        {
            mTemplate.Deserialize(packageBytes);

            switch (mTemplate.Op)
            {
                case UserNameListPackage.EOperate.Push:

                    var fetchedItem = mTemplate.UserNameInfoList[0];
                    UpdateUserInfoList(fetchedItem);

                    break;
                case UserNameListPackage.EOperate.Fetch:

                    mTemplate.Op = UserNameListPackage.EOperate.Push;
                    mTemplate.UserNameInfoList.Clear();
                    mTemplate.UserNameInfoList.Add(new UserNameInfoData()
                    {
                        IPAddress = mUDPBoxContainer.SelfIPAddress.ToString(),
                        BeginPort = mUDPBoxContainer.UdpBoxBeginPort,
                        EndPort = mUDPBoxContainer.UdpBoxEndPort,
                        UserName = SelfUserName,
                    });
                    udpBox.SendMessage(mTemplate.Serialize(), ipEndPoint);

                    break;
                case UserNameListPackage.EOperate.FetchAll:

                    mTemplate.Op = UserNameListPackage.EOperate.RestoreList;
                    mTemplate.UserNameInfoList.Clear();
                    for (int i = 0, iMax = UserNameInfoList.Count; i < iMax; i++)
                    {
                        var item = UserNameInfoList[i];
                        mTemplate.UserNameInfoList.Add(new UserNameInfoData()
                        {
                            IPAddress = item.IPAddress,
                            BeginPort = item.BeginPort,
                            EndPort = item.EndPort,
                            UserName = item.UserName,
                        });
                    }
                    udpBox.SendMessage(mTemplate.Serialize(), ipEndPoint);

                    break;

                case UserNameListPackage.EOperate.RestoreList:

                    UserNameInfoList.Clear();
                    for (int i = 0, iMax = mTemplate.UserNameInfoList.Count; i < iMax; i++)
                    {
                        var item = mTemplate.UserNameInfoList[i];

                        UserNameInfoList.Add(new UserNameInfoData()
                        {
                            IPAddress = item.IPAddress,
                            BeginPort = item.BeginPort,
                            EndPort = item.EndPort,
                            UserName = item.UserName
                        });
                    }

                    break;
                default:
                    break;
            }
        }

        void WorkThreadOperateLoop()
        {
            var deltaTime = UDPBoxUtility.GetDeltaTime(mLastWorkThreadTime);

            if (mRefreshDelayTimer <= 0)
            {
                if (mUDPBoxContainer.IsMaster)
                {
                    UpdateUserInfoList();
                }
                else
                {
                    if (mUDPBoxContainer.State == UDPBoxContainer.EState.HasServer)
                    {
                        mTemplate.Op = UserNameListPackage.EOperate.FetchAll;
                        mUDPBoxContainer.SendUDPMessage(mTemplate.Serialize(), mUDPBoxContainer.MasterIPConnectInfo.IPEndPoint);
                    }
                }

                mRefreshDelayTimer = REFRESH_DELAY;
            }
            else
            {
                mRefreshDelayTimer -= deltaTime;
            }

            mLastWorkThreadTime = DateTime.Now.Ticks;
        }

        void UpdateUserInfoList(UserNameInfoData fetchedUser)
        {
            var userNameInfo = fetchedUser;

            var userInfoIndex = UserNameInfoList.FindIndex(m => m.IPAddress == userNameInfo.IPAddress
                && m.BeginPort == userNameInfo.BeginPort && m.EndPort == userNameInfo.EndPort);

            if (userInfoIndex == -1)
            {
                UserNameInfoList.Add(new UserNameInfoData()
                {
                    IPAddress = userNameInfo.IPAddress,
                    BeginPort = userNameInfo.BeginPort,
                    EndPort = userNameInfo.EndPort,
                    UserName = userNameInfo.UserName
                });
            }
            else
            {
                UserNameInfoList[userInfoIndex] = new UserNameInfoData()
                {
                    IPAddress = userNameInfo.IPAddress,
                    BeginPort = userNameInfo.BeginPort,
                    EndPort = userNameInfo.EndPort,
                    UserName = userNameInfo.UserName
                };
            }
        }

        void UpdateUserInfoList()
        {
            var clientIpConnectList = mUDPBoxContainer.ClientIPConnectList;
            for (int i = 0, iMax = clientIpConnectList.Count; i < iMax; i++)
            {
                var item = clientIpConnectList[i];

                if (!item.Valid) continue;
                if (!item.IsClientEstablished) continue;

                var address = item.IPEndPoint.Address.ToString();
                var port = item.IPEndPoint.Port;
                var userNameInfoIndex = UserNameInfoList.FindIndex(m => m.IPAddress == address
                        && m.BeginPort <= port && m.EndPort > port);

                if (userNameInfoIndex == -1)
                {
                    mTemplate.Op = UserNameListPackage.EOperate.Fetch;
                    mTemplate.UserNameInfoList.Clear();
                    mUDPBoxContainer.SendUDPMessage(mTemplate.Serialize(), item.IPEndPoint);
                }
            }

            for (int i = UserNameInfoList.Count - 1; i >= 0; i--)
            {
                var marked_userNameInfo = UserNameInfoList[i];

                if (marked_userNameInfo.UserName == SelfUserName) continue;

                var foundClientObject = clientIpConnectList.Find(m => m.Valid
                    && m.IPEndPoint.Address.ToString() == marked_userNameInfo.IPAddress
                    && m.IPEndPoint.Port >= marked_userNameInfo.BeginPort
                    && m.IPEndPoint.Port < marked_userNameInfo.EndPort);

                if (!foundClientObject.Valid)
                    UserNameInfoList.RemoveAt(i);
            }
            var selfIPAddress = mUDPBoxContainer.SelfIPAddress.ToString();
            var selfBeginPort = mUDPBoxContainer.UdpBoxBeginPort;
            var selfEndPort = mUDPBoxContainer.UdpBoxEndPort;

            var selfUserNameInfoIndex = UserNameInfoList
                .FindIndex(m => m.IPAddress == selfIPAddress
                    && m.BeginPort == selfBeginPort && m.EndPort == selfEndPort);

            if (selfUserNameInfoIndex == -1)
            {
                UserNameInfoList.Add(new UserNameInfoData()
                {
                    IPAddress = selfIPAddress,
                    BeginPort = selfBeginPort,
                    EndPort = selfEndPort,
                    UserName = SelfUserName,
                });
            }
            else
            {
                UserNameInfoList[selfUserNameInfoIndex] = new UserNameInfoData()
                {
                    IPAddress = selfIPAddress,
                    BeginPort = selfBeginPort,
                    EndPort = selfEndPort,
                    UserName = SelfUserName,
                };
            }
        }
    }
}
