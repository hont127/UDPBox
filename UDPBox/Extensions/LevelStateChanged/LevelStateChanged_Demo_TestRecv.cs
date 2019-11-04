using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public class LevelStateChanged_Demo_TestRecv : MonoBehaviour
    {
        public LevelStateChanged_RegisterMono levelStateChange;


        void OnEnable()
        {
            levelStateChange.OnStateChanged += OnStateChanged;
        }

        void OnDisable()
        {
            levelStateChange.OnStateChanged -= OnStateChanged;
        }

        void OnStateChanged(int newState, string arg)
        {
            Debug.Log("recv newState: " + newState + " arg: " + arg);
        }
    }
}
