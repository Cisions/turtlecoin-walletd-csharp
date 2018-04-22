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

        public void Exit()
        {
            Mobile.Exit(true);
            Wallet.Exit(true);
            Daemon.Exit(true);
        }
    }
}
