using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Hont.UDPBoxPackage
{
    class LargePackageHandler : HandlerBase
    {
        public const float REFRESH_DELAY = 1f;
        public float RECYCLE_TIME = 90f;

        public struct SegmentInfo
        {
            public byte[] Bytes { get; set; }
        }

        public struct LargePackageInfo
        {
            public short ConcretePackageID { get; set; }
            public long ConcretePackageUUID { get; set; }
            public SegmentInfo[] SegmentInfoArray { get; set; }

            public float RecycleTimer { get; set; }
        }

        LargePackage mTemplate;
        long mLastWorkThreadTime;
        float mRefreshDelayTimer;

        List<LargePackageInfo> mLargePackageInfoList;


        public LargePackageHandler(byte[] packageHead)
        {
            mTemplate = new LargePackage(packageHead);

            mLargePackageInfoList = new List<LargePackageInfo>(4);
        }

        public override void OnRegistedToUDPBox(UDPBox udpBox)
        {
            base.OnRegistedToUDPBox(udpBox);

            udpBox.RegistWorkThreadOperate(WorkThreadOperateLoop);
        }

        protected override short[] GetCacheProcessableID()
        {
            return new short[] { UDPBoxUtility.LARGE_PACKAGE_ID };
        }

        protected void WorkThreadOperateLoop()
        {
            var deltaTime = UDPBoxUtility.GetDeltaTime(mLastWorkThreadTime);

            if (mRefreshDelayTimer <= 0)
            {
                for (int i = mLargePackageInfoList.Count - 1; i >= 0; i--)
                {
                    var largePackageInfo = mLargePackageInfoList[i];
                    largePackageInfo.RecycleTimer -= deltaTime;

                    if (largePackageInfo.RecycleTimer <= 0f)
                        mLargePackageInfoList.RemoveAt(i);
                }

                mRefreshDelayTimer = REFRESH_DELAY;
            }
            else
            {
                mRefreshDelayTimer -= deltaTime;
            }

            mLastWorkThreadTime = DateTime.Now.Ticks;
        }

        public override void Process(UDPBox udpBox, byte[] packageBytes, IPEndPoint ipEndPoint)
        {
            if (!mTemplate.Deserialize(packageBytes)) return;

            var package_id = mTemplate.ConcretePackageID;
            var package_uuid = mTemplate.ConcretePackageUUID;
            var package_segment_id = mTemplate.SegmentID;
            var package_segment_total_count = mTemplate.SegmentTotalCount;

            var targetPackageInfoIndex = mLargePackageInfoList.FindIndex(m => m.ConcretePackageID == package_id
                                && m.ConcretePackageUUID == package_uuid);

            if (targetPackageInfoIndex == -1)
            {
                var largePackageInfo = new LargePackageInfo();
                largePackageInfo.ConcretePackageID = package_id;
                largePackageInfo.ConcretePackageUUID = package_uuid;
                largePackageInfo.RecycleTimer = RECYCLE_TIME;
                largePackageInfo.SegmentInfoArray = new SegmentInfo[package_segment_total_count];

                mLargePackageInfoList.Add(largePackageInfo);
                targetPackageInfoIndex = mLargePackageInfoList.Count - 1;
            }

            var item = mLargePackageInfoList[targetPackageInfoIndex];
            item.SegmentInfoArray[package_segment_id].Bytes = mTemplate.BytesList.ToArray();
            mLargePackageInfoList[targetPackageInfoIndex] = item;

            var flag = true;
            for (int i = 0; i < item.SegmentInfoArray.Length; i++)
            {
                if (item.SegmentInfoArray[i].Bytes == null)
                {
                    flag = false;
                    break;
                }
            }

            if (flag)
            {
                mLargePackageInfoList.RemoveAt(targetPackageInfoIndex);
                List<byte> combine_bytes = new List<byte>();
                for (int i = 0; i < item.SegmentInfoArray.Length; i++)
                    combine_bytes.AddRange(item.SegmentInfoArray[i].Bytes);

                udpBox.ProcessPackage(combine_bytes.ToArray(), ipEndPoint);
            }
        }
    }
}
