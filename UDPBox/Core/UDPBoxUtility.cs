using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;

namespace Hont.UDPBoxPackage
{
    public static class UDPBoxUtility
    {
        public const short BROADCAST_PACKAGE_ID = -1;
        public const short ESTABLISH_CONNECT_ID = -2;
        public const short PING_PONG_ID = -3;
        public const short ACK_ID = -4;
        public const short LARGE_PACKAGE_ID = -5;

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

        public static long GetUUID()
        {
            var buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }

        public static byte[][] ConvertToLargePackageBytes(Package package, int segmentByteLength)
        {
            var packageID = package.ID;
            var packageUUID = GetUUID();
            var bytes = package.Serialize();
            var segmentCount = (int)Math.Ceiling(bytes.Length / (double)segmentByteLength);
            var wait_return_bytes = new List<byte[]>();

            for (int i = 0; i < segmentCount; i++)
            {
                var currentSegment_b = (i + 1) * segmentByteLength;

                if (bytes.Length < currentSegment_b)
                {
                    var currentSegment_a = i * segmentByteLength;
                    var sub_bytes = new byte[bytes.Length - currentSegment_a];
                    Array.Copy(bytes, i * segmentByteLength, sub_bytes, 0, sub_bytes.Length);

                    var largePackage = new LargePackage(package.HeadBytes);
                    largePackage.ConcretePackageID = packageID;
                    largePackage.ConcretePackageUUID = packageUUID;
                    largePackage.SegmentID = segmentCount - 1;
                    largePackage.SegmentTotalCount = segmentCount;
                    largePackage.BytesList = new List<byte>(sub_bytes);
                    wait_return_bytes.Add(largePackage.Serialize());
                }
                else
                {
                    var sub_bytes = new byte[segmentByteLength];
                    Array.Copy(bytes, i * segmentByteLength, sub_bytes, 0, sub_bytes.Length);

                    var largePackage = new LargePackage(package.HeadBytes);
                    largePackage.ConcretePackageID = packageID;
                    largePackage.ConcretePackageUUID = packageUUID;
                    largePackage.SegmentID = i;
                    largePackage.SegmentTotalCount = segmentCount;
                    largePackage.BytesList = new List<byte>(sub_bytes);
                    wait_return_bytes.Add(largePackage.Serialize());
                }
            }

            return wait_return_bytes.ToArray();
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
    }
}
