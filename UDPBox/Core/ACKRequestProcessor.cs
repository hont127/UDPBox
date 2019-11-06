using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace Hont.UDPBoxPackage
{
    public class ACKRequestProcessor
    {
        const int MAGIC_NUMBER_MARK_COUNT = 32;

        public struct WaitACKInfo
        {
            public byte[] Bytes { get; set; }
            public int MagicNumber { get; set; }
            public int PackageID { get; set; }

            public float Timer { get; set; }

            public string IPEndPoint_IPAddress { get; set; }
            public int IPEndPoint_Port { get; set; }
        }

        public struct PackageCompareInfo
        {
            public short ID { get; set; }
            public ushort MagicNumber { get; set; }
        }


        float mACKDetectDelayTime;
        UDPBox mUdpBox;
        ACKPackage mACKPackageTemplate;
        List<WaitACKInfo> mWaitACKInfoList;
        Queue<PackageCompareInfo> mMagicNumberMarkQueue;

        long mLastTick;


        public ACKRequestProcessor(UDPBox udpBox, float ackDetectDelayTime = 0.3f)
        {
            mWaitACKInfoList = new List<WaitACKInfo>(32);
            mMagicNumberMarkQueue = new Queue<PackageCompareInfo>(MAGIC_NUMBER_MARK_COUNT);

            mUdpBox = udpBox;
            mACKDetectDelayTime = ackDetectDelayTime;
        }

        public void Initialization()
        {
            mWaitACKInfoList.Clear();
            mACKPackageTemplate = new ACKPackage(mUdpBox.PackageHeadBytes);
            mUdpBox.OnSendMessage += OnSendMessage;
            mUdpBox.RegistMessageIntercept(OnACKMessageIntercept);
            mUdpBox.RegistWorkThreadOperate(ACKWaitPackageLogicUpdate);
        }

        public void Release()
        {
            mUdpBox.OnSendMessage -= OnSendMessage;
            mUdpBox.UnregistMessageIntercept(OnACKMessageIntercept);
            mUdpBox.UnregistWorkThreadOperate(ACKWaitPackageLogicUpdate);
        }

        void OnSendMessage(byte[] bytes, IPEndPoint ipEndPoint)
        {
            short package_type = 0;
            ushort package_magicNumber = 0;
            short package_id = 0;
            UDPBoxUtility.GetPackageBaseInfo(bytes, mUdpBox.PackageHeadBytes
                , out package_type, out package_magicNumber, out package_id);

            if (package_type == (short)EPackageType.Need_Ack_Session)
            {
                var flag = false;
                for (int i = 0, iMax = mWaitACKInfoList.Count; i < iMax; i++)
                {
                    var item = mWaitACKInfoList[i];

                    if (item.PackageID == package_id && item.MagicNumber == package_magicNumber)
                    {
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    mWaitACKInfoList.Add(new WaitACKInfo()
                    {
                        Bytes = bytes,
                        MagicNumber = package_magicNumber,
                        PackageID = package_id,
                        IPEndPoint_IPAddress = ipEndPoint.Address.ToString(),
                        IPEndPoint_Port = ipEndPoint.Port,
                        Timer = mACKDetectDelayTime,
                    });
                }
            }
        }

        bool OnACKMessageIntercept(UDPBox.MessageInterceptInfo messageInterceptInfo)
        {
            var result = false;

            short type = 0;
            ushort magicNumber = 0;
            short id = 0;
            UDPBoxUtility.GetPackageBaseInfo(messageInterceptInfo.Bytes, mUdpBox.PackageHeadBytes, out type, out magicNumber, out id);

            //--------------------------------------------------------------------
            var flag = false;
            lock (mMagicNumberMarkQueue)
            {
                foreach (var item in mMagicNumberMarkQueue)
                {
                    if (item.ID == id && item.MagicNumber == magicNumber)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag) return true;
            //--------------------------------------------------------------------Package repeat.

            if (type == (short)EPackageType.Need_Ack_Session)//Received generic ack package.
            {
                lock (mMagicNumberMarkQueue)
                {
                    if (mMagicNumberMarkQueue.Count > MAGIC_NUMBER_MARK_COUNT)
                        mMagicNumberMarkQueue.Dequeue();
                    mMagicNumberMarkQueue.Enqueue(new PackageCompareInfo() { ID = id, MagicNumber = magicNumber });
                }

                mACKPackageTemplate.ACK_ID = id;
                mACKPackageTemplate.ACK_MagicNumber = magicNumber;
                mUdpBox.SendMessage(mACKPackageTemplate.Serialize(), messageInterceptInfo.IPEndPoint);
            }
            else if (id == UDPBoxUtility.ACK_ID)//Received ack package.
            {
                mACKPackageTemplate.Deserialize(messageInterceptInfo.Bytes);

                var ack_id = mACKPackageTemplate.ACK_ID;
                var ack_MagicNumber = mACKPackageTemplate.ACK_MagicNumber;

                lock (mWaitACKInfoList)
                {
                    for (int i = mWaitACKInfoList.Count - 1; i >= 0; i--)
                    {
                        var item = mWaitACKInfoList[i];

                        if (item.MagicNumber == ack_MagicNumber && item.PackageID == ack_id)
                        {
                            mWaitACKInfoList.RemoveAt(i);
                            break;
                        }
                    }
                }

                result = true;
            }

            return result;
        }

        void ACKWaitPackageLogicUpdate()
        {
            lock (mWaitACKInfoList)
            {
                var deltaTime = UDPBoxUtility.GetDeltaTime(mLastTick);

                for (int i = 0, iMax = mWaitACKInfoList.Count; i < iMax; i++)
                {
                    var item = mWaitACKInfoList[i];

                    if (item.Timer <= 0f)
                    {
                        mUdpBox.SendMessage(item.Bytes, new IPEndPoint(IPAddress.Parse(item.IPEndPoint_IPAddress), item.IPEndPoint_Port));
                        item.Timer = mACKDetectDelayTime;
                    }
                    else
                    {
                        item.Timer -= deltaTime;
                    }

                    mWaitACKInfoList[i] = item;
                }

                mLastTick = DateTime.Now.Ticks;
            }
        }
    }
}