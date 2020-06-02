using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using Hont.UDPBoxPackage;

using UnityEngine;

namespace Hont.UDPBoxExtensions
{
    public class UDPBoxContainer_HallController : MonoBehaviour
    {
        public const float ROOM_ALIVE_DURATION = 8f;

        public enum EState { HallWait, RoomHost, RoomClient }

        public struct RoomInfo
        {
            public string IPAddress { get; set; }
            public string RoomName { get; set; }
            public int BeginPort { get; set; }
            public int EndPort { get; set; }
            public float AliveTimer { get; set; }


            public override bool Equals(object obj)
            {
                var compare = (RoomInfo)obj;
                return IPAddress == compare.IPAddress
                    && RoomName == compare.RoomName
                    && BeginPort == compare.BeginPort
                    && EndPort == compare.EndPort;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public UDPBoxContainer_Mono udpBoxContainer;
        public int sendBroadcastPort;
        public int recvBroadcastPort;
        public bool autoSetup;

        string mPackageHeadString;
        string mRoomName;
        EState mCurrentState;
        UDPBoxContainer mContainer;
        UDPBoxContainer_HallDataPackage mHallPackage;
        UDPBoxBroadcast mBroadcast;
        UDPBox_UDPClient mUdpClient;

        List<RoomInfo> mRoomInfoList;
        public List<RoomInfo> RoomInfoList { get { return mRoomInfoList; } }
        public string RoomName { get { return mRoomName; } }
        public EState State { get { return mCurrentState; } }
        public string BroadcastNetPrefixIP { get; set; } = "192.168.1.";

        public event Action OnUdpBoxContainerInitialization;
        public event Action OnUdpBoxContainerRelease;


        public void CreateRoom(string roomName)
        {
            mRoomName = roomName;

            mHallPackage.BeginPort = mContainer.UdpBoxBeginPort;
            mHallPackage.EndPort = mContainer.UdpBoxEndPort;
            mHallPackage.IPAddress = mContainer.SelfIPAddress.ToString();
            mHallPackage.RoomName = roomName;

            mBroadcast.ReleaseThread();
            mBroadcast.ResetState();
            mBroadcast.StartBroadcast(mHallPackage.Serialize(), BroadcastNetPrefixIP);

            ToRoomHostMode();
        }

        public void JoinRoom(RoomInfo roomInfo)
        {
            ToRoomClientMode();

            var establishedPackage = new EstablishConnectPackage(mContainer.PackageHeadBytes);
            establishedPackage.SenderType = EstablishConnectPackage.ESenderType.Client;
            establishedPackage.BeginPort = mContainer.UdpBoxBeginPort;
            establishedPackage.EndPort = mContainer.UdpBoxEndPort;
            establishedPackage.IpAddress = mContainer.SelfIPAddress.ToString();
            establishedPackage.IsReceipt = false;

            mContainer.SendUDPMessageToRandomPort(establishedPackage.Serialize(), roomInfo.BeginPort, roomInfo.EndPort, IPAddress.Parse(roomInfo.IPAddress));
            Debug.Log("[JoinRoom]A send to B: " + roomInfo.IPAddress + " self port,begin: " + mContainer.UdpBoxBeginPort + " self port,end: " + mContainer.UdpBoxEndPort);
        }

        public void ExitRoom()
        {
            ToHallWaitMode();
        }

        public void Initialization(string packageHeadString, UDPBoxContainer container)
        {
            mContainer = container;
            mPackageHeadString = packageHeadString;
            mHallPackage = new UDPBoxContainer_HallDataPackage(UDPBoxUtility.ToBuffer(mPackageHeadString));

            mUdpClient = UDPBoxFactory.GeterateUdpClient(recvBroadcastPort);
            mBroadcast = new UDPBoxBroadcast(mUdpClient, sendBroadcastPort);

            ToHallWaitMode();
        }

        public void Release()
        {
            mUdpClient?.Dispose();
            mBroadcast?.ReleaseThread();
            mContainer?.Release();
            OnUdpBoxContainerRelease?.Invoke();
        }

        void Awake()
        {
            mRoomInfoList = new List<RoomInfo>(32);
        }

        void Start()
        {
            if (autoSetup)
            {
                if (udpBoxContainer.useInternalBroadcastLogic)
                    throw new NotSupportedException("Not support internal broadcast logic!");

                Initialization(udpBoxContainer.packageHeadString, udpBoxContainer.GetNativeContainer());
            }
        }

        void Update()
        {
            for (int i = 0, iMax = mRoomInfoList.Count; i < iMax; i++)
            {
                var item = mRoomInfoList[i];
                item.AliveTimer -= UnityEngine.Time.deltaTime;
                mRoomInfoList[i] = item;
            }

            for (int i = mRoomInfoList.Count - 1; i >= 0; i--)
            {
                if (mRoomInfoList[i].AliveTimer <= 0f)
                    mRoomInfoList.RemoveAt(i);
            }
        }

        void OnDestroy()
        {
            Release();
        }

        void ToRoomClientMode()
        {
            mBroadcast.ReleaseThread();
            mBroadcast.ResetState();
            mCurrentState = EState.RoomClient;
            mContainer.RestartUDPBoxContainer(mPackageHeadString, false, false);
            OnUdpBoxContainerInitialization?.Invoke();
        }

        void ToRoomHostMode()
        {
            mCurrentState = EState.RoomHost;
            mContainer.RestartUDPBoxContainer(mPackageHeadString, false, true);
            OnUdpBoxContainerInitialization?.Invoke();
        }

        void ToHallWaitMode()
        {
            mContainer.Release();
            OnUdpBoxContainerRelease?.Invoke();

            mRoomName = "";

            mCurrentState = EState.HallWait;

            mBroadcast.ReleaseThread();
            mBroadcast.ResetState();

            mBroadcast.ListenBroadcast((bytes, endPoint) =>
            {
                if (UDPBoxUtility.PackageIsBroken(bytes, mContainer.PackageHeadBytes)) return;
                if (!UDPBoxUtility.ComparePackageID(bytes, mContainer.PackageHeadBytes, mHallPackage.ID)) return;

                if (!mHallPackage.Deserialize(bytes)) return;

                var ipEndPoint = new IPEndPoint(IPAddress.Parse(mHallPackage.IPAddress), mHallPackage.BeginPort);
                if (ipEndPoint.Address.Equals(mContainer.SelfIPAddress)
                    && ipEndPoint.Port < mContainer.UdpBoxEndPort && ipEndPoint.Port >= mContainer.UdpBoxBeginPort) return;
                //Avoid self connect to the self.

                var roomInfo = new RoomInfo()
                {
                    RoomName = mHallPackage.RoomName,
                    BeginPort = mHallPackage.BeginPort,
                    EndPort = mHallPackage.EndPort,
                    IPAddress = mHallPackage.IPAddress,
                    AliveTimer = ROOM_ALIVE_DURATION,
                };

                if (!mRoomInfoList.Contains(roomInfo))
                {
                    mRoomInfoList.Add(roomInfo);
                }
                else
                {
                    var itemIndex = mRoomInfoList.FindIndex(m => m.Equals(roomInfo));
                    roomInfo.AliveTimer = ROOM_ALIVE_DURATION;
                    mRoomInfoList[itemIndex] = roomInfo;
                }
            });
        }
    }
}
