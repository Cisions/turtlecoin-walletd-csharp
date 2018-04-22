using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using TurtleCoinAPI;

namespace Mobile_Wallet_Server
{
    class Program
    {
        public TurtleCoin _session;

        public void DaemonLog(object sender, LogEventArgs e)
        {
            Console.WriteLine("Daemon:\t{0}", e.Message);
        }

        public void WalletLog(object sender, LogEventArgs e)
        {
            Console.WriteLine("Wallet:\t{0}", e.Message);
        }

        public void MobileLog(object sender, LogEventArgs e) { }

        public void Error(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("Error:\t{0}", e.ErrorCode);
        }

        public void OnDaemonReady(object sender, EventArgs e)
        {
            (sender as Daemon).SendRequestAsync(RequestMethod.GET_CURRENCY_ID, new RequestParams { }, out JObject Result);
            Console.WriteLine("Result: {0}", Result.ToString());
        }

        public void OnDaemonUpdate(object sender, EventArgs e)
        {
            if (!_session.Daemon.Synced)
                Console.WriteLine("Syncing - {0} / {1}", _session.Daemon.Height, _session.Daemon.NetworkHeight);
        }

        public void OnDaemonConnect(object sender, EventArgs e) { }
        public void OnDaemonDisconnect(object sender, EventArgs e) { }
        public void OnWalletSynced(object sender, EventArgs e) { }
        public void OnWalletUpdate(object sender, EventArgs e) { }
        public void OnWalletConnect(object sender, EventArgs e) { }
        public void OnWalletDisconnect(object sender, EventArgs e) { }
        public void OnMobileRequest(object sender, EventArgs e) { }

        public static void Main(string[] args)
        {
            // Run program
            Program p = new Program();
            p.Run();

            // Await key press to close
            Console.ReadKey();

            // Clean up session
            p._session.Exit();
        }

        public async Task Run()
        {
            // Create a new session
            _session = new TurtleCoin();
            _session.Daemon.RefreshRate = 5000;
            _session.Wallet.RefreshRate = 5000;

            // Assign daemon event handlers
            _session.Daemon.Log += DaemonLog;
            _session.Daemon.Error += Error;
            _session.Daemon.OnConnect += OnDaemonConnect;
            _session.Daemon.OnReady += OnDaemonReady;
            _session.Daemon.OnUpdate += OnDaemonUpdate;
            _session.Daemon.OnDisconnect += OnDaemonDisconnect;

            // Assign wallet event handlers
            _session.Wallet.Log += WalletLog;
            _session.Wallet.Error += Error;
            _session.Wallet.OnConnect += OnWalletConnect;
            _session.Wallet.OnSynced += OnWalletSynced;
            _session.Wallet.OnUpdate += OnWalletUpdate;
            _session.Wallet.OnDisconnect += OnWalletDisconnect;

            // Assign mobile event handlers
            _session.Mobile.Log += MobileLog;
            _session.Mobile.Error += Error;
            _session.Mobile.OnRequestReceived += OnMobileRequest;

            // Initialize daemon
            await _session.Daemon.InitializeAsync("c:/turtlecoin/turtlecoind.exe", 11898);

            // Begin daemon update loop
            await _session.Daemon.BeginUpdateAsync();
            
            // Initialize wallet
            await _session.Wallet.InitializeAsync(_session.Daemon, "c:/turtlecoin/walletd.exe", "c:/turtlecoin/testwallet.wallet", "12345", 11911);

            // Begin wallet update loop
            await _session.Wallet.BeginUpdateAsync();

            /*
            // Initialize mobile
            await _session.Mobile.InitializeAsync(_session.Wallet, 19991);

            // Begin mobile listening
            await _session.Mobile.BeginUpdateAsync();
            */

            // Delay infinitely to stay alive
            await Task.Delay(-1);
        }
    }
}
