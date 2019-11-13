using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class UserNameListPackage : Package
    {
        public enum EOperate { Push, Fetch, FetchAll, RestoreList }
        public EOperate Op { get; set; }
        public List<UserNameInfoData> UserNameInfoList { get; set; }
        public override bool EnabledCompress => true;


        public UserNameListPackage(byte[] headBytes)
            : base(headBytes)
        {
            base.ID = UDPBoxExtensionConsts.USER_NAME_LIST;

            UserNameInfoList = new List<UserNameInfoData>(12);

            Args = new PackageArgument[]
            {
                new PackageArgument_Int(),
                new PackageArgument_StringList(),
                new PackageArgument_IntList(),
                new PackageArgument_IntList(),
                new PackageArgument_StringList(),
            };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_Int).Value = (int)Op;

            var ipAddressList = (Args[1] as PackageArgument_StringList).Value;
            var beginPortList = (Args[2] as PackageArgument_IntList).Value;
            var endPortList = (Args[3] as PackageArgument_IntList).Value;
            var userNameList = (Args[4] as PackageArgument_StringList).Value;

            ipAddressList.Clear();
            beginPortList.Clear();
            endPortList.Clear();
            userNameList.Clear();

            for (int i = 0, iMax = UserNameInfoList.Count; i < iMax; i++)
            {
                var userNameInfo = UserNameInfoList[i];

                ipAddressList.Add(userNameInfo.IPAddress);
                beginPortList.Add(userNameInfo.BeginPort);
                endPortList.Add(userNameInfo.EndPort);
                userNameList.Add(userNameInfo.UserName);
            }

            return base.Serialize();
        }

        public override bool Deserialize(byte[] bytes)
        {
            var result = base.Deserialize(bytes);
            if (!result) return result;

            Op = (EOperate)(Args[0] as PackageArgument_Int).Value;

            var ipAddressList = (Args[1] as PackageArgument_StringList).Value;
            var beginPortList = (Args[2] as PackageArgument_IntList).Value;
            var endPortList = (Args[3] as PackageArgument_IntList).Value;
            var userNameList = (Args[4] as PackageArgument_StringList).Value;

            UserNameInfoList.Clear();

            for (int i = 0, iMax = ipAddressList.Count; i < iMax; i++)
            {
                var ipAddress = ipAddressList[i];
                var beginPort = beginPortList[i];
                var endPort = endPortList[i];
                var userName = userNameList[i];

                UserNameInfoList.Add(new UserNameInfoData()
                {
                    IPAddress = ipAddress,
                    BeginPort = beginPort,
                    EndPort = endPort,
                    UserName = userName,
                });
            }

            return result;
        }
    }
}
