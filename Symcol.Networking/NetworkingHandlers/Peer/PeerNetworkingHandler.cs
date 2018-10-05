﻿using System;
using osu.Framework.Logging;
using Symcol.Networking.NetworkingClients;
using Symcol.Networking.Packets;

namespace Symcol.Networking.NetworkingHandlers.Peer
{
    public class PeerNetworkingHandler : NetworkingHandler
    {
        #region Fields

        /// <summary>
        /// Call this when we connect to a Host
        /// </summary>
        public Action<Host> OnConnectedToHost;

        public ConnectionStatues ConnectionStatues { get; protected set; }

        #endregion

        public PeerNetworkingHandler()
        {
            OnAddressChange += (ip, port) => NetworkingClient = new UdpNetworkingClient(ip + ":" + port);
        }

        #region Update Loop

        protected override void Update()
        {
            base.Update();

            foreach (Packet p in ReceivePackets())
                HandlePackets(p);
        }

        /// <summary>
        /// Handle any packets we got before sending them to OnPackerReceive
        /// </summary>
        /// <param name="packet"></param>
        protected override void HandlePackets(Packet packet)
        {
            Logger.Log($"Recieved a Packet from {NetworkingClient.EndPoint}", LoggingTarget.Network, LogLevel.Debug);

            switch (packet)
            {
                case ConnectedPacket _:
                    ConnectionStatues = ConnectionStatues.Connected;
                    Logger.Log("Connected to server!", LoggingTarget.Network);
                    OnConnectedToHost?.Invoke(new Host());
                    break;
                case DisconnectPacket _:
                    Logger.Log("Server shutting down!", LoggingTarget.Network);
                    break;
                case TestPacket _:
                    SendToServer(new TestPacket());
                    break;
            }

            OnPacketReceive?.Invoke(packet);
        }

        #endregion

        #region Packet and Client Helper Functions

        protected override Packet SignPacket(Packet packet)
        {
            if (packet is ConnectPacket c)
                c.Gamekey = Gamekey;
            return packet;
        }

        #endregion

        #region Send Functions

        public virtual void SendToServer(Packet packet)
        {
            NetworkingClient.SendPacket(SignPacket(packet));
        }

        #endregion

        #region Network Actions

        /// <summary>
        /// Starts the connection proccess to Host / Server
        /// </summary>
        public virtual void Connect()
        {
            // ReSharper disable once InvertIf
            if (true)//ConnectionStatues <= ConnectionStatues.Disconnected)
            {
                Logger.Log($"Attempting to connect to {NetworkingClient.Address}", LoggingTarget.Network);
                SendToServer(new ConnectPacket());
                ConnectionStatues = ConnectionStatues.Connecting;
            }
            //else
                //Logger.Log("We are already connecting, please wait for us to fail before retrying!", LoggingTarget.Network);
        }

        public virtual void Disconnect()
        {
            if (ConnectionStatues >= ConnectionStatues.Connecting)
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
