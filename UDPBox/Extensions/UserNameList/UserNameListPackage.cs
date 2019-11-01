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
                new PackageArgument_StringList(),
            };
        }

        public override byte[] Serialize()
        {
            (Args[0] as PackageArgument_Int).Value = (int)Op;

            var ipAddressList = (Args[1] as PackageArgument_StringList).Value;
            var portList = (Args[2] as PackageArgument_IntList).Value;
            var userNameList = (Args[3] as PackageArgument_StringList).Value;

            ipAddressList.Clear();
            portList.Clear();
            userNameList.Clear();

            for (int i = 0, iMax = UserNameInfoList.Count; i < iMax; i++)
            {
                var userNameInfo = UserNameInfoList[i];

                ipAddressList.Add(userNameInfo.IPAddress);
                portList.Add(userNameInfo.Port);
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
            var portList = (Args[2] as PackageArgument_IntList).Value;
            var userNameList = (Args[3] as PackageArgument_StringList).Value;

            UserNameInfoList.Clear();

            for (int i = 0, iMax = ipAddressList.Count; i < iMax; i++)
            {
                var ipAddress = ipAddressList[i];
                var port = portList[i];
                var userName = userNameList[i];

                UserNameInfoList.Add(new UserNameInfoData()
                {
                    IPAddress = ipAddress,
                    Port = port,
                    UserName = userName,
                });
            }

            return result;
        }
    }
}
