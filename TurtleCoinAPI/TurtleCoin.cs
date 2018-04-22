using System;
using System.Threading.Tasks;

namespace TurtleCoinAPI
{
    public partial class TurtleCoin
    {
        /// <summary>
        /// Creates a session
        /// </summary>
        public TurtleCoin()
        {
            Daemon = new Daemon();
            Wallet = new Wallet();
            Mobile = new Mobile();
        }

        /// <summary>
        /// Cleans up and exits from the session, saving daemon and wallet progress
        /// </summary>
        public async Task Exit(bool ForceExit = false)
        {
            await Mobile.Exit(ForceExit);
            await Wallet.Exit(ForceExit);
            await Daemon.Exit(ForceExit);
        }
    }
}
