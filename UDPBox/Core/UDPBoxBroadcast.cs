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
        public static int BROAD_CAST_INTERVAL_MS = 1000;

        int mBroadcastPort;
        Thread mThread;
        UDPBox_UDPClient mUdpClient;
        bool mIsReleased;


        public UDPBoxBroadcast(UDPBox_UDPClient udpClient, int broadcastPort)
        {
            mBroadcastPort = broadcastPort;
            mUdpClient = udpClient;
            mIsReleased = false;
        }

        public void ResetState()
        {
            mIsReleased = false;
        }

        public void ReleaseThread()
        {
            mIsReleased = true;

            if (mThread != null && mThread.IsAlive)
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
                        try
                        {
                            var ipEndPoint = new IPEndPoint(IPAddress.Parse(netPrefixIP + "255"), mBroadcastPort);
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

                    Thread.Sleep(BROAD_CAST_INTERVAL_MS);
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
                    catch (ThreadAbortException)
                    {
                    }
                    catch (Exception e)
                    {
                        if (!mIsReleased)
                            Debug.LogError("Broadcast error: " + e);
                    }

                    Thread.Sleep(BROAD_CAST_INTERVAL_MS);
                }
            });
            mThread.Start();
        }
    }
}
