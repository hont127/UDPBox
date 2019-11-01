using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public class GetMasterClientCount_Demo_TestRequest : MonoBehaviour
    {
        [SerializeField]
        UDPBoxContainer_Mono container;


        void OnGUI()
        {
            if (GUILayout.Button("Send"))
            {
                container.SendUDPMessage(new GetMasterClientCountPackage(UDPBoxUtility.DefaultHeadBytes).Serialize()
                    , container.MasterIPConnectInfo.IPEndPoint);
            }
        }
    }
}
