using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TurtleCoinAPI
{
    public partial class Mobile
    {
        // The listener
        private TcpListener
            Listener;

        private IPAddress
            IpAddress;

        private int
            Port;

        private string
            HostName;

        public bool
            Listening = false;

        // Event handlers
        public EventHandler<LogEventArgs> Log;
        public EventHandler<ErrorEventArgs> Error;
        public EventHandler
            OnConnectionReceived,
            OnConnectionClosed,
            OnRequestReceived;

        // For canceling async loops
        private CancellationTokenSource
            CancellationSource = new CancellationTokenSource();
    }
}
