using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Hont.UDPBoxPackage
{
    public class UDPBOX_UDPClient : IDisposable
    {
        static IPEndPoint Any_IPEndPoint = new IPEndPoint(IPAddress.Any, 0);
        static IPEndPoint IPv6Any_IPEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);

        int mBufferCacheLength;

        Socket m_ClientSocket;
        bool m_Active;
        byte[] m_Buffer;
        AddressFamily m_Family;
        bool m_CleanedUp;
        bool m_IsBroadcast;

        public Socket Client
        {
            get
            {
                return this.m_ClientSocket;
            }
            set
            {
                this.m_ClientSocket = value;
            }
        }
        protected bool Active
        {
            get
            {
                return this.m_Active;
            }
            set
            {
                this.m_Active = value;
            }
        }
        public int Available
        {
            get
            {
                return this.m_ClientSocket.Available;
            }
        }
        public short Ttl
        {
            get
            {
                return this.m_ClientSocket.Ttl;
            }
            set
            {
                this.m_ClientSocket.Ttl = value;
            }
        }
        public bool DontFragment
        {
            get
            {
                return this.m_ClientSocket.DontFragment;
            }
            set
            {
                this.m_ClientSocket.DontFragment = value;
            }
        }
        public bool MulticastLoopback
        {
            get
            {
                return this.m_ClientSocket.MulticastLoopback;
            }
            set
            {
                this.m_ClientSocket.MulticastLoopback = value;
            }
        }
        public bool EnableBroadcast
        {
            get
            {
                return this.m_ClientSocket.EnableBroadcast;
            }
            set
            {
                this.m_ClientSocket.EnableBroadcast = value;
            }
        }
        public bool ExclusiveAddressUse
        {
            get
            {
                return this.m_ClientSocket.ExclusiveAddressUse;
            }
            set
            {
                this.m_ClientSocket.ExclusiveAddressUse = value;
            }
        }


        public UDPBOX_UDPClient() : this(AddressFamily.InterNetwork, 4096)
        {
        }

        public UDPBOX_UDPClient(AddressFamily family, int bufferCacheLength)
        {
            m_Buffer = new byte[bufferCacheLength];
            m_Family = AddressFamily.InterNetwork;
            if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
                throw new ArgumentException("net_protocol_invalid_family");
            m_Family = family;
            CreateClientSocket();
        }

        public UDPBOX_UDPClient(int port) : this(port, 1024, AddressFamily.InterNetwork)
        {
        }

        public UDPBOX_UDPClient(int port, int bufferCacheLength, AddressFamily family)
        {
            mBufferCacheLength = bufferCacheLength;

            m_Buffer = new byte[mBufferCacheLength];
            m_Family = AddressFamily.InterNetwork;

            m_Family = family;
            IPEndPoint localEP = null;
            if (m_Family == AddressFamily.InterNetwork)
            {
                localEP = new IPEndPoint(IPAddress.Any, port);
            }
            else
            {
                localEP = new IPEndPoint(IPAddress.IPv6Any, port);
            }
            CreateClientSocket();
            Client.Bind(localEP);
        }

        public UDPBOX_UDPClient(IPEndPoint localEP, int bufferCacheLength, int buffer)
        {
            mBufferCacheLength = bufferCacheLength;
            m_Buffer = new byte[mBufferCacheLength];
            m_Family = AddressFamily.InterNetwork;
            if (localEP == null)
            {
                throw new ArgumentNullException("localEP");
            }
            m_Family = localEP.AddressFamily;
            CreateClientSocket();
            Client.Bind(localEP);
        }

        public UDPBOX_UDPClient(string hostname, int port, int bufferCacheLength)
        {
            mBufferCacheLength = bufferCacheLength;
            m_Buffer = new byte[mBufferCacheLength];
            m_Family = AddressFamily.InterNetwork;
            if (hostname == null)
                throw new ArgumentNullException("hostname");

            Connect(hostname, port);
        }

        public void AllowNatTraversal(bool allowed)
        {
            if (allowed)
            {
                m_ClientSocket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
                return;
            }
            m_ClientSocket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted);
        }

        public void Close()
        {
            Dispose(true);
        }

        void FreeResources()
        {
            if (m_CleanedUp)
                return;

            var client = Client;
            if (client != null)
            {
                if (client.Connected)
                    client.Shutdown(SocketShutdown.Both);
                client.Close();
                Client = null;
            }
            m_CleanedUp = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                FreeResources();
                GC.SuppressFinalize(this);
            }
        }

        public void Connect(string hostname, int port)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (hostname == null)
            {
                throw new ArgumentNullException("hostname");
            }

            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
            Exception ex = null;
            Socket socket = null;
            Socket socket2 = null;
            try
            {
                if (m_ClientSocket == null)
                {
                    if (Socket.OSSupportsIPv4)
                    {
                        socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    }
                    if (Socket.OSSupportsIPv6)
                    {
                        socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                    }
                }
                IPAddress[] array = hostAddresses;
                for (int i = 0; i < array.Length; i++)
                {
                    IPAddress iPAddress = array[i];
                    try
                    {
                        if (m_ClientSocket == null)
                        {
                            if (iPAddress.AddressFamily == AddressFamily.InterNetwork && socket2 != null)
                            {
                                socket2.Connect(iPAddress, port);
                                m_ClientSocket = socket2;
                                if (socket != null)
                                {
                                    socket.Close();
                                }
                            }
                            else
                            {
                                if (socket != null)
                                {
                                    socket.Connect(iPAddress, port);
                                    m_ClientSocket = socket;
                                    if (socket2 != null)
                                    {
                                        socket2.Close();
                                    }
                                }
                            }
                            m_Family = iPAddress.AddressFamily;
                            m_Active = true;
                            break;
                        }
                        if (iPAddress.AddressFamily == this.m_Family)
                        {
                            Connect(new IPEndPoint(iPAddress, port));
                            m_Active = true;
                            break;
                        }
                    }
                    catch (Exception ex2)
                    {
                        if (IsFatal(ex2))
                        {
                            throw;
                        }
                        ex = ex2;
                    }
                }
            }
            catch (Exception ex3)
            {
                if (IsFatal(ex3))
                {
                    throw;
                }
                ex = ex3;
            }
            finally
            {
                if (!m_Active)
                {
                    if (socket != null)
                    {
                        socket.Close();
                    }
                    if (socket2 != null)
                    {
                        socket2.Close();
                    }
                    if (ex != null)
                    {
                        throw ex;
                    }
                    throw new SocketException((int)SocketError.NotConnected);
                }
            }
        }

        public void Connect(IPAddress addr, int port)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (addr == null)
            {
                throw new ArgumentNullException("addr");
            }
            IPEndPoint endPoint = new IPEndPoint(addr, port);
            Connect(endPoint);
        }

        public void Connect(IPEndPoint endPoint)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint");
            }
            CheckForBroadcast(endPoint.Address);
            Client.Connect(endPoint);
            m_Active = true;
        }

        void CheckForBroadcast(IPAddress ipAddress)
        {
            if (Client != null && !m_IsBroadcast && IsBroadcast(ipAddress))
            {
                m_IsBroadcast = true;
                Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            }
        }

        public int Send(byte[] dgram, int bytes, IPEndPoint endPoint)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (dgram == null)
            {
                throw new ArgumentNullException("dgram");
            }
            if (m_Active && endPoint != null)
            {
                throw new InvalidOperationException("net_udpconnected");
            }
            if (endPoint == null)
            {
                return Client.Send(dgram, 0, bytes, SocketFlags.None);
            }
            CheckForBroadcast(endPoint.Address);
            return Client.SendTo(dgram, 0, bytes, SocketFlags.None, endPoint);
        }

        public int Send(byte[] dgram, int bytes, string hostname, int port)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (dgram == null)
            {
                throw new ArgumentNullException("dgram");
            }
            if (m_Active && (hostname != null || port != 0))
            {
                throw new InvalidOperationException("net_udpconnected");
            }
            if (hostname == null || port == 0)
            {
                return Client.Send(dgram, 0, bytes, SocketFlags.None);
            }
            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
            int num = 0;
            while (num < hostAddresses.Length && hostAddresses[num].AddressFamily != m_Family)
            {
                num++;
            }
            if (hostAddresses.Length == 0 || num == hostAddresses.Length)
            {
                throw new ArgumentException("hostname");
            }
            CheckForBroadcast(hostAddresses[num]);
            IPEndPoint remoteEP = new IPEndPoint(hostAddresses[num], port);
            return Client.SendTo(dgram, 0, bytes, SocketFlags.None, remoteEP);
        }

        public int Send(byte[] dgram, int bytes)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (dgram == null)
            {
                throw new ArgumentNullException("dgram");
            }
            if (!m_Active)
            {
                throw new InvalidOperationException("net_notconnected");
            }
            return Client.Send(dgram, 0, bytes, SocketFlags.None);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public IAsyncResult BeginSend(byte[] datagram, int bytes, IPEndPoint endPoint, AsyncCallback requestCallback, object state)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (datagram == null)
            {
                throw new ArgumentNullException("datagram");
            }
            if (bytes > datagram.Length || bytes < 0)
            {
                throw new ArgumentOutOfRangeException("bytes");
            }
            if (m_Active && endPoint != null)
            {
                throw new InvalidOperationException("net_udpconnected");
            }
            if (endPoint == null)
            {
                return Client.BeginSend(datagram, 0, bytes, SocketFlags.None, requestCallback, state);
            }
            CheckForBroadcast(endPoint.Address);
            return Client.BeginSendTo(datagram, 0, bytes, SocketFlags.None, endPoint, requestCallback, state);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public IAsyncResult BeginSend(byte[] datagram, int bytes, string hostname, int port, AsyncCallback requestCallback, object state)
        {
            if (m_Active && (hostname != null || port != 0))
            {
                throw new InvalidOperationException("net_udpconnected");
            }
            IPEndPoint endPoint = null;
            if (hostname != null && port != 0)
            {
                IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
                int num = 0;
                while (num < hostAddresses.Length && hostAddresses[num].AddressFamily != this.m_Family)
                {
                    num++;
                }
                if (hostAddresses.Length == 0 || num == hostAddresses.Length)
                {
                    throw new ArgumentException("net_invalidAddressList");
                }
                CheckForBroadcast(hostAddresses[num]);
                endPoint = new IPEndPoint(hostAddresses[num], port);
            }
            return BeginSend(datagram, bytes, endPoint, requestCallback, state);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public IAsyncResult BeginSend(byte[] datagram, int bytes, AsyncCallback requestCallback, object state)
        {
            return BeginSend(datagram, bytes, null, requestCallback, state);
        }

        public int EndSend(IAsyncResult asyncResult)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (m_Active)
            {
                return Client.EndSend(asyncResult);
            }
            return Client.EndSendTo(asyncResult);
        }

        public byte[] Receive(ref IPEndPoint remoteEP)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            EndPoint endPoint;
            if (m_Family == AddressFamily.InterNetwork)
            {
                endPoint = Any_IPEndPoint;
            }
            else
            {
                endPoint = IPv6Any_IPEndPoint;
            }
            int num = this.Client.ReceiveFrom(this.m_Buffer, mBufferCacheLength, SocketFlags.None, ref endPoint);
            remoteEP = (IPEndPoint)endPoint;
            if (num < mBufferCacheLength)
            {
                byte[] array = new byte[num];
                Buffer.BlockCopy(m_Buffer, 0, array, 0, num);
                return array;
            }
            return this.m_Buffer;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public IAsyncResult BeginReceive(AsyncCallback requestCallback, object state)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            EndPoint endPoint;
            if (m_Family == AddressFamily.InterNetwork)
            {
                endPoint = Any_IPEndPoint;
            }
            else
            {
                endPoint = IPv6Any_IPEndPoint;
            }
            return Client.BeginReceiveFrom(m_Buffer, 0, mBufferCacheLength, SocketFlags.None, ref endPoint, requestCallback, state);
        }

        public byte[] EndReceive(IAsyncResult asyncResult, ref IPEndPoint remoteEP)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            EndPoint endPoint;
            if (m_Family == AddressFamily.InterNetwork)
            {
                endPoint = Any_IPEndPoint;
            }
            else
            {
                endPoint = IPv6Any_IPEndPoint;
            }
            int num = this.Client.EndReceiveFrom(asyncResult, ref endPoint);
            remoteEP = (IPEndPoint)endPoint;
            if (num < mBufferCacheLength)
            {
                byte[] array = new byte[num];
                Buffer.BlockCopy(m_Buffer, 0, array, 0, num);
                return array;
            }
            return m_Buffer;
        }

        public void JoinMulticastGroup(IPAddress multicastAddr)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            if (multicastAddr.AddressFamily != this.m_Family)
            {
                throw new ArgumentException("net_protocol_invalid_multicast_family");
            }
            if (m_Family == AddressFamily.InterNetwork)
            {
                MulticastOption optionValue = new MulticastOption(multicastAddr);
                Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
                return;
            }
            IPv6MulticastOption optionValue2 = new IPv6MulticastOption(multicastAddr);
            Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, optionValue2);
        }

        public void JoinMulticastGroup(IPAddress multicastAddr, IPAddress localAddress)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (m_Family != AddressFamily.InterNetwork)
            {
                throw new SocketException();
            }
            MulticastOption optionValue = new MulticastOption(multicastAddr, localAddress);
            Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
        }

        public void JoinMulticastGroup(int ifindex, IPAddress multicastAddr)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            if (ifindex < 0)
            {
                throw new ArgumentException("net_value_cannot_be_negative");
            }
            if (m_Family != AddressFamily.InterNetworkV6)
            {
                throw new SocketException();
            }
            IPv6MulticastOption optionValue = new IPv6MulticastOption(multicastAddr, (long)ifindex);
            Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, optionValue);
        }

        public void JoinMulticastGroup(IPAddress multicastAddr, int timeToLive)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            JoinMulticastGroup(multicastAddr);
            Client.SetSocketOption((m_Family == AddressFamily.InterNetwork) ? SocketOptionLevel.IP : SocketOptionLevel.IPv6, SocketOptionName.MulticastTimeToLive, timeToLive);
        }

        public void DropMulticastGroup(IPAddress multicastAddr)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            if (multicastAddr.AddressFamily != this.m_Family)
            {
                throw new ArgumentException("net_protocol_invalid_multicast_family");
            }
            if (m_Family == AddressFamily.InterNetwork)
            {
                MulticastOption optionValue = new MulticastOption(multicastAddr);
                Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, optionValue);
                return;
            }
            IPv6MulticastOption optionValue2 = new IPv6MulticastOption(multicastAddr);
            Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, optionValue2);
        }

        public void DropMulticastGroup(IPAddress multicastAddr, int ifindex)
        {
            if (m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            if (ifindex < 0)
            {
                throw new ArgumentException("net_value_cannot_be_negative");
            }
            if (m_Family != AddressFamily.InterNetworkV6)
            {
                throw new SocketException((int)SocketError.OperationNotSupported);
            }
            IPv6MulticastOption optionValue = new IPv6MulticastOption(multicastAddr, (long)ifindex);
            Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, optionValue);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public Task<int> SendAsync(byte[] datagram, int bytes)
        {
            return Task<int>.Factory.FromAsync<byte[], int>(new Func<byte[], int, AsyncCallback, object, IAsyncResult>(BeginSend), new Func<IAsyncResult, int>(EndSend), datagram, bytes, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public Task<int> SendAsync(byte[] datagram, int bytes, IPEndPoint endPoint)
        {
            return Task<int>.Factory.FromAsync<byte[], int, IPEndPoint>(new Func<byte[], int, IPEndPoint, AsyncCallback, object, IAsyncResult>(BeginSend), new Func<IAsyncResult, int>(EndSend), datagram, bytes, endPoint, null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public Task<int> SendAsync(byte[] datagram, int bytes, string hostname, int port)
        {
            return Task<int>.Factory.FromAsync((AsyncCallback callback, object state) => BeginSend(datagram, bytes, hostname, port, callback, state), new Func<IAsyncResult, int>(EndSend), null);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
        public Task<UdpReceiveResult> ReceiveAsync()
        {
            return Task<UdpReceiveResult>.Factory.FromAsync((AsyncCallback callback, object state) => BeginReceive(callback, state), delegate (IAsyncResult ar)
            {
                IPEndPoint remoteEndPoint = null;
                byte[] buffer = EndReceive(ar, ref remoteEndPoint);
                return new UdpReceiveResult(buffer, remoteEndPoint);
            }, null);
        }

        void CreateClientSocket()
        {
            Client = new Socket(m_Family, SocketType.Dgram, ProtocolType.Udp);
        }

        bool IsFatal(Exception exception)
        {
            return exception != null && (exception is OutOfMemoryException || exception is StackOverflowException || exception is ThreadAbortException);
        }

        bool IsBroadcast(IPAddress ipAddress)
        {
            return m_Family != AddressFamily.InterNetworkV6 && ipAddress.Equals(IPAddress.Broadcast);
        }
    }
}
