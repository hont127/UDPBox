using Hont.UDPBoxPackage;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    [DefaultExecutionOrder(5)]
    public class LevelStateChanged_RegisterMono : MonoBehaviour
    {
        public UDPBoxContainer_Mono udpBoxContainer;
        LevelStateChangedHandler mHandler;

        public event Action<int, string> OnStateChanged;


        void OnEnable()
        {
            StartCoroutine(WaitAndInitialization());
        }

        void OnDisable()
        {
            mHandler.OnStateChanged -= OnStateChange;
            udpBoxContainer.UDPBox.UnregistHandler(mHandler);
        }

        IEnumerator WaitAndInitialization()
        {
            yield return new WaitUntil(() => udpBoxContainer.State != UDPBoxContainer.EState.NoStart);

            mHandler = new LevelStateChangedHandler(udpBoxContainer.PackageHeadBytes);
            mHandler.OnStateChanged += OnStateChange;
            udpBoxContainer.UDPBox.RegistHandler(mHandler);
        }

        void OnStateChange(int newStateID, string args)
        {
            OnStateChanged?.Invoke(newStateID, args);
        }
    }
}
