using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Hont.UDPBoxPackage
{
    public static class UDPBoxUtility
    {
        public const short BROADCAST_PACKAGE_ID = -1;
        public const short ESTABLISH_SERVER_CONNECT_ID = -2;
        public const short PING_PONG_ID = -3;
        public const short ACK_ID = -4;

        static byte[] mCache4BytesArray;
        static byte[] mCache2BytesArray;

        public static string DefaultHead { get; set; } = "Hont.UDPBox";
        public static byte[] DefaultHeadBytes { get; set; }


        static UDPBoxUtility()
        {
            DefaultHeadBytes = ToBuffer(DefaultHead);

            mCache2BytesArray = new byte[2];
            mCache4BytesArray = new byte[4];
        }

        static bool IsCorrentIP(string ip)
        {
            var pattrn = @"(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])";
            if (System.Text.RegularExpressions.Regex.IsMatch(ip, pattrn)) return true;
            else return false;
        }

        public static IPAddress GetSelfIP(string ipBroadcastPrefix = "192.168.1.")
        {
            var resultIP = default(IPAddress);
            var ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (var ip in ips)
            {
                if (IsCorrentIP(ip.ToString()) && ip.ToString().Contains(ipBroadcastPrefix))
                {
                    resultIP = ip;
                    break;
                }
            }

            if (resultIP == null)
                resultIP = IPAddress.Parse("127.0.0.1");

            return resultIP;
        }

        public static string[] FindBroadcastIPV4Prefixs()
        {
            var resultList = new List<string>(4);

            var ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (var ip in ips)
            {
                if (IsCorrentIP(ip.ToString()))
                {
                    var addressStr = ip.ToString();
                    var lastIndexOf = addressStr.LastIndexOf(".");
                    var addressPrefixStr = addressStr.Substring(0, lastIndexOf + 1);

                    resultList.Add(addressPrefixStr);
                }
            }

            return resultList.ToArray();
        }

        public static bool CheckByteHead(byte[] source, byte[] checkHead)
        {
            var result = true;

            for (int i = 0; i < checkHead.Length; i++)
            {
                if (checkHead[i] != source[i])
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        public static float GetDeltaTime(long lastTick)
        {
            const float MILLISECOND_PER_SECOND = 1000f;

            var deltaMS_Int = lastTick == 0 ? 0 : (DateTime.Now.Ticks - lastTick) / TimeSpan.TicksPerMillisecond;
            var deltaMS = deltaMS_Int / MILLISECOND_PER_SECOND;

            return deltaMS;
        }

        public static byte[] ToBuffer(string content)
        {
            return Encoding.ASCII.GetBytes(content);
        }

        public static string FromBuffer(byte[] bufferCache, int length)
        {
            return Encoding.ASCII.GetString(bufferCache, 0, length);
        }

        public static int GetPackageID(byte[] packageBytes, byte[] packageHeadBytes)
        {
            var seek = packageHeadBytes.Length + 4;

            mCache2BytesArray[0] = packageBytes[seek];
            mCache2BytesArray[1] = packageBytes[seek + 1];

            return BitConverter.ToInt16(mCache2BytesArray, 0);
        }

        public static void GetPackageBaseInfo(byte[] packageBytes, byte[] packageHeadBytes, out short type, out ushort magicNumber, out short id)
        {
            var seek = packageHeadBytes.Length;

            mCache2BytesArray[0] = packageBytes[seek];
            mCache2BytesArray[1] = packageBytes[seek + 1];

            type = BitConverter.ToInt16(mCache2BytesArray, 0);

            seek += 2;
            mCache2BytesArray[0] = packageBytes[seek];
            mCache2BytesArray[1] = packageBytes[seek + 1];
            magicNumber = BitConverter.ToUInt16(mCache2BytesArray, 0);

            seek += 2;
            mCache2BytesArray[0] = packageBytes[seek];
            mCache2BytesArray[1] = packageBytes[seek + 1];
            id = BitConverter.ToInt16(mCache2BytesArray, 0);
        }

        public static bool ComparePackageID(byte[] packageBytes, byte[] packageHeadBytes, short identify)
        {
            var id = GetPackageID(packageBytes, packageHeadBytes);
            if (id == identify) return true;
            return false;
        }

        public static bool ComparePackageID(byte[] packageBytes, byte[] packageHeadBytes, short[] identifys)
        {
            var id = GetPackageID(packageBytes, packageHeadBytes);

            for (int i = 0; i < identifys.Length; i++)
                if (identifys[i] == id) return true;
            return false;
        }

        public static bool PackageIsBroken(byte[] packageBytes, byte[] packageHeadBytes)
        {
            if (!CheckByteHead(packageBytes, packageHeadBytes)) return true;

            var beforeBytes = packageHeadBytes.Length + 6;
            mCache4BytesArray[0] = packageBytes[beforeBytes];
            mCache4BytesArray[1] = packageBytes[beforeBytes + 1];
            mCache4BytesArray[2] = packageBytes[beforeBytes + 2];
            mCache4BytesArray[3] = packageBytes[beforeBytes + 3];

            var sourceLength = BitConverter.ToUInt32(mCache4BytesArray, 0);
            var realLength = packageBytes.Length - (beforeBytes + 4);

            if (sourceLength != realLength)
                return true;

            return false;
        }

        public static UDPBOX_UDPClient GeterateUdpClient(int port)
        {
            var mUdpClient = new UDPBOX_UDPClient(port);
            mUdpClient.Client.SendTimeout = 1000;
            mUdpClient.Client.ReceiveTimeout = 10000;

            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            mUdpClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
            //屏蔽目标机器没有接收逻辑时报错。

            return mUdpClient;
        }

        public static UDPBOX_UDPClient[] GeterateUdpClientsArray(int beginPort, int endPort)
        {
            var result = new UDPBOX_UDPClient[endPort - beginPort];

            for (int i = 0; i < result.Length; i++)
                result[i] = GeterateUdpClient(beginPort + i);

            return result;
        }
    }
}
