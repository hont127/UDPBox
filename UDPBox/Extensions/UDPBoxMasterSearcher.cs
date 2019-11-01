using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    [DefaultExecutionOrder(5)]
    public class UDPBoxMasterSearcher : MonoBehaviour
    {
        [SerializeField]
        UDPBoxContainer_Mono udpBoxContainer = null;

        [SerializeField]
        bool isDebug = true;


        void OnEnable()
        {
            StartCoroutine(IPSearch());
        }

        IEnumerator IPSearch()
        {
            var searched_flag = false;
            var spareIPPrefixs = UDPBoxUtility.FindBroadcastIPV4Prefixs();
            for (int i = 0, iMax = spareIPPrefixs.Length; i < iMax && !searched_flag; i++)
            {
                var ip_prefix = spareIPPrefixs[i];

                if (isDebug)
                    Debug.Log("Searcher current try to used the ip address: " + ip_prefix);

                udpBoxContainer.broadcastNetprefixIP = ip_prefix;
                udpBoxContainer.RestartUDPBoxContainer();

                for (int i_wait = 0; i_wait < 5 && !searched_flag; i_wait++)
                {
                    yield return new WaitForSeconds(10f);

                    if (isDebug)
                        Debug.Log("Try count: " + i + " state: " + udpBoxContainer.State);

                    if (udpBoxContainer.State == UDPBoxContainer.EState.HasServer)
                    {
                        searched_flag = true;
                        break;
                    }
                }

                if (!searched_flag)
                    udpBoxContainer.ReleaseUDPBoxContainer();
            }
        }
    }
}
