using Hont.UDPBoxPackage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    [DefaultExecutionOrder(5)]
    public class UserNameList_RegisterMono : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;
        public List<UserNameInfoData> userNameList;
        public string selfUserName = "Tom";
        public bool isDebug;

        UserNameListHandler mHandler;


        void OnEnable()
        {
            StartCoroutine(WaitAndInitialization());
        }

        void OnDisable()
        {
            udpBoxContainer.UDPBox.UnregistHandler(mHandler);
        }

        void OnGUI()
        {
            if (!isDebug) return;

            for (int i = 0, iMax = userNameList.Count; i < iMax; i++)
            {
                var item = userNameList[i];

                GUILayout.Box("index: " + i + " item.IPAddress: " + item.IPAddress + " port: " + item.Port + " name: " + item.UserName);
            }
        }

        IEnumerator WaitAndInitialization()
        {
            yield return new WaitUntil(() => udpBoxContainer.State != UDPBoxContainer.EState.NoStart);

            mHandler = new UserNameListHandler(udpBoxContainer.GetNativeContainer());
            mHandler.SelfUserName = selfUserName;
            userNameList = mHandler.UserNameInfoList;

            udpBoxContainer.UDPBox.RegistHandler(mHandler);
        }
    }
}
