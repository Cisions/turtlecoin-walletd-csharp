using System;
using System.Collections.Generic;
using System.Net;

namespace TurtleCoinAPI
{
    public partial class WebServer
    {
        // The listener
        private HttpListener Listener { get; set; }

        // Port to listen on
        private int Port { get; set; }

        // Listen status
        public bool Listening { get; private set; }

        // Event handlers
        public EventHandler<TurtleCoinLogEventArgs> Log;
        public EventHandler<TurtleCoinErrorEventArgs> Error;
        public EventHandler
            OnConnectionReceived,
            OnConnectionClosed,
            OnRequestReceived;

        // Daemon and wallet to hook to
        private Wallet  Wallet = new Wallet();
        private Daemon Daemon = new Daemon();

        // Response
        public string Response { get; private set; }
        
        // Private variables
        private bool Initialized { get; set; }
        private Dictionary<string, Endpoint> Endpoints { get; set; }
    }
}
