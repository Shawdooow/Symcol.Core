﻿#region usings

using System;
using System.Collections.Generic;
using System.Net;
using osu.Framework.Logging;
using Sym.Networking.NetworkingClients;
using Sym.Networking.Packets;

#endregion

namespace Sym.Networking.NetworkingHandlers.Peer
{
    public class PeerNetworkingHandler : PeerNetworkingHandler<Host>
    {
        protected override Host GetClient(IPEndPoint end)
        {
            return new Host(end);
        }
    }

    public abstract class PeerNetworkingHandler<T> : NetworkingHandler<T>
        where T : Host
    {
        #region Fields

        public T Host;

        /// <summary>
        /// Call this when we connect to a Host
        /// </summary>
        public Action<T> OnConnectedToHost;

        /// <summary>
        /// Gets hit when we get a Packet
        /// </summary>
        public Action<PacketInfo<T>> OnPacketReceive;

        #endregion

        protected PeerNetworkingHandler()
        {
            OnAddressChange += (ip, port) =>
            {
                if (Udp)
                    UdpNetworkingClient = new UdpNetworkingClient(ip + ":" + port);
                TcpNetworkingClient = new TcpNetworkingClient(ip + ":" + port);
                Host = GetClient(TcpNetworkingClient.EndPoint);
            };
            OnUdpChange += value =>
            {
                if (value)
                {
                    UdpNetworkingClient = new UdpNetworkingClient(IP + ":" + Port);
                }
                else
                {
                    UdpNetworkingClient.Dispose();
                    UdpNetworkingClient = null;
                }
            };
        }

        #region Update Loop

        /// <summary>
        /// Handle any packets we got before sending them to OnPackerReceive
        /// </summary>
        /// <param name="info"></param>
        protected override void PacketReceived(PacketInfo<T> info)
        {
            switch (info.Packet)
            {
                case ConnectedPacket _:
                    Host.Statues = ConnectionStatues.Connected;
                    Logger.Log("Connected to server!", LoggingTarget.Network);
                    break;
                case DisconnectPacket _:
                    Logger.Log("Server shutting down!", LoggingTarget.Network);
                    break;
                case TestPacket _:
                    SendToServer(new TestPacket());
                    break;
            }

            OnPacketReceive?.Invoke(info);
        }

        #endregion

        #region Packet and Client Helper Functions

        protected override List<PacketInfo<T>> GetPackets()
        {
            List<PacketInfo<T>> packets = new List<PacketInfo<T>>();

            restart:

            CheckQueue(TcpNetworkingClient.TcpClient, TcpNetworkingClient.NextPacketSize, out int s);
            TcpNetworkingClient.NextPacketSize = s;

            if (read()) goto restart;

            return packets;

            bool read()
            {
                if (TcpNetworkingClient.NextPacketSize > 0 && TcpNetworkingClient.Available >= TcpNetworkingClient.NextPacketSize)
                {
                    packets.Add(new PacketInfo<T>(Host, TcpNetworkingClient.GetPacket()));
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region Send Functions

        public virtual void SendToServer(Packet packet)
        {
            TcpNetworkingClient.SendPacket(SignPacket(packet));
        }

        #endregion

        #region Network Actions

        /// <summary>
        /// Starts the connection proccess to Host / Server
        /// </summary>
        public virtual void Connect()
        {

        }

        public virtual void Disconnect()
        {
            if (Host.Statues >= ConnectionStatues.Connecting)
                SendToServer(new DisconnectPacket());
            else
                Logger.Log("We are not connected!", LoggingTarget.Network);
        }

        #endregion

        protected override void Dispose(bool isDisposing)
        {
            SendToServer(new DisconnectPacket());
            base.Dispose(isDisposing);
        }
    }
}
