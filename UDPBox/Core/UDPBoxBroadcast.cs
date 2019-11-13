using System;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace Hont.UDPBoxPackage
{
    public class UDPBoxBroadcast
    {
        int mBroadcastPort;
        Thread mThread;
        UdpClient mUdpClient;
        bool mIsReleased;


        public UDPBoxBroadcast(UdpClient udpClient, int broadcastPort)
        {
            mBroadcastPort = broadcastPort;
            mUdpClient = udpClient;
            mIsReleased = false;
        }

        public void ReleaseThread()
        {
            mIsReleased = true;
            mThread.Abort();
        }

        public void StartBroadcast(byte[] bytes, string netPrefixIP = "192.168.1.")
        {
            mThread = new Thread(() =>
            {
                while (!mIsReleased)
                {
                    lock (mUdpClient)
                    {
                        for (int i = 0; i < 255; i++)
                        {
                            if (mIsReleased)
                                break;

                            try
                            {
                                var ipEndPoint = new IPEndPoint(IPAddress.Parse(netPrefixIP + i), mBroadcastPort);
                                mUdpClient.Send(bytes, bytes.Length, ipEndPoint);
                            }
                            catch (SocketException socket_exception) when (socket_exception.ErrorCode == 10051)
                            {
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }

                    Thread.Sleep(1000);
                }
            });
            mThread.Start();
        }

        public void ListenBroadcast(Action<byte[], IPEndPoint> onReceived)
        {
            var remotePoint = new IPEndPoint(IPAddress.Any, 0);
            mThread = new Thread(() =>
            {
                while (!mIsReleased)
                {
                    try
                    {
                        var bytes = mUdpClient.Receive(ref remotePoint);
                        onReceived(bytes, remotePoint);
                    }
                    catch (SocketException socket_exception) when (socket_exception.ErrorCode == 10060 || socket_exception.ErrorCode == 10004)
                    {
                    }
                    catch (Exception e)
                    {
                        if (!mIsReleased)
                            Debug.LogError("Broadcast error: " + e);
                    }

                    Thread.Sleep(1000);
                }
            });
            mThread.Start();
        }
    }
}
