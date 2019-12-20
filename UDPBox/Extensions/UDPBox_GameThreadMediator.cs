using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public class UDPBox_GameThreadMediator : MonoBehaviour
    {
        static bool mIsDestroying;

        static UDPBox_GameThreadMediator mInstance;
        public static UDPBox_GameThreadMediator Instance
        {
            get
            {
                if (mIsDestroying) return null;

                if (mInstance == null)
                {
                    var go = new GameObject("[UDPBox_GameThreadMediator]");
                    mInstance = go.AddComponent<UDPBox_GameThreadMediator>();
                    DontDestroyOnLoad(go);
                }

                return mInstance;
            }
        }

        Queue<Action> mActionQueue;
        Queue<Action> mLateUpdateActionQueue;


        public static void InitializationInUnityGameThread()
        {
            UDPBox_GameThreadMediator.Instance.GetHashCode();
        }

        public void EnqueueToUpdateQueue(Action content)
        {
            mActionQueue.Enqueue(content);
        }

        public void EnqueueToLateUpdateQueue(Action content)
        {
            mLateUpdateActionQueue.Enqueue(content);
        }

        void Awake()
        {
            mActionQueue = new Queue<Action>(32);
            mLateUpdateActionQueue = new Queue<Action>(32);
        }

        void OnDestroy()
        {
            mIsDestroying = true;
        }

        void Update()
        {
            lock (mActionQueue)
            {
                for (int i = 0; i < mActionQueue.Count; i++)
                    mActionQueue.Dequeue()?.Invoke();
            }
        }

        void LateUpdate()
        {
            lock (mActionQueue)
            {
                for (int i = 0; i < mLateUpdateActionQueue.Count; i++)
                    mLateUpdateActionQueue.Dequeue()?.Invoke();
            }
        }
    }
}
