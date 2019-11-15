using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hont.UDPBoxPackage;

namespace Hont.UDPBoxExtensions
{
    public sealed class HardDiskUDPBoxLogger : MonoBehaviour, IUDPBoxLogger
    {
        static bool mIsDestroying;
        static HardDiskUDPBoxLogger mInstance;
        public static HardDiskUDPBoxLogger Instance
        {
            get
            {
                if (mIsDestroying) return null;

                if (mInstance == null)
                {
                    mInstance = new GameObject("HardDiskUDPBoxLogger").AddComponent<HardDiskUDPBoxLogger>();
                    DontDestroyOnLoad(mInstance.gameObject);
                }

                return mInstance;
            }
        }

        StringBuilder mStringBuilder;


        void OnEnable()
        {
            mStringBuilder = new StringBuilder();
        }

        void OnDisable()
        {
            var sbStr = mStringBuilder.ToString();
            var fileName = "UDPBoxLog.txt";
            if (File.Exists(fileName))
                File.Delete(fileName);
            File.AppendAllText(fileName, sbStr);
        }

        void OnDestroy()
        {
            mIsDestroying = true;
        }

        void IUDPBoxLogger.Log(string content, EUDPBoxLogType type)
        {
            var timeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            switch (type)
            {
                case EUDPBoxLogType.Log:
                    mStringBuilder.AppendLine($"[{timeStr}][Log]{content}");
                    break;
                case EUDPBoxLogType.Warning:
                    mStringBuilder.AppendLine($"[{timeStr}][Warning]{content}");
                    break;
                case EUDPBoxLogType.Error:
                    mStringBuilder.AppendLine($"[{timeStr}][Error]{content}");
                    break;
                default:
                    break;
            }
        }
    }
}
