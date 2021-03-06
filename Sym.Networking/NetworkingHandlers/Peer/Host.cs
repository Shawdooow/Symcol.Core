﻿#region usings

using System.Net;
using osu.Framework.Logging;

#endregion

namespace Sym.Networking.NetworkingHandlers.Peer
{
    public class Host : Client
    {
        public override ConnectionStatues Statues
        {
            get => base.Statues;
            set
            {
                if (base.Statues != value)
                {
                    base.Statues = value;
                    switch (value)
                    {
                        case ConnectionStatues.Connecting:
                            Logger.Log($"Connecting to Host {EndPoint.Address}...", LoggingTarget.Network);
                            break;
                        case ConnectionStatues.Connected:
                            Logger.Log($"Connected to Host {EndPoint.Address}!", LoggingTarget.Network);
                            break;
                        case ConnectionStatues.Disconnected:
                            Logger.Log($"Disconnected from Host {EndPoint.Address}s", LoggingTarget.Network);
                            break;
                    }
                }
            }
        }

        public Host(IPEndPoint end)
            : base(end)
        {
        }
    }
}
