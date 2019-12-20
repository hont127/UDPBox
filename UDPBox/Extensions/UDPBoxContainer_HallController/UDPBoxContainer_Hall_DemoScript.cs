using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public class UDPBoxContainer_Hall_DemoScript : MonoBehaviour
    {
        public UDPBoxContainer_HallController hallController1;
        public UDPBoxContainer_HallController hallController2;


        void OnGUI()
        {
            GUILayout.Box(hallController1.State.ToString());

            for (int i = 0; i < hallController1.RoomInfoList.Count; i++)
            {
                var item = hallController1.RoomInfoList[i];

                if (GUILayout.Button("[hall1]Room: " + item.RoomName + " IPAddress: " + item.IPAddress + " timer: " + item.AliveTimer))
                {
                    hallController1.JoinRoom(item);
                }
            }

            if (GUILayout.Button("Hall2 - Create Room1"))
            {
                hallController2.CreateRoom("[hall2]Room1");
            }

            if (GUILayout.Button("Hall2 - Exit Room1"))
            {
                hallController1.ExitRoom();
            }
        }
    }
}
