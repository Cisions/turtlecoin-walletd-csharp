using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TurtleCoinAPI
{
    public partial class Mobile
    {
        /// <summary>
        /// Initializes a mobile listener
        /// </summary>
        /// <param name="Port">Port to listen on</param>
        public Task InitializeAsync(Wallet Wallet, int Port = 8085)
        {
            // Set local variables
            this.Port = Port;
            HostName = Dns.GetHostName();

            // Obtain IP address
            IPHostEntry ipHostInfo = Dns.GetHostEntry(HostName);
            for (int i = 0; i < ipHostInfo.AddressList.Length; ++i)
            {
                if (ipHostInfo.AddressList[i].AddressFamily ==
                  AddressFamily.InterNetwork)
                {
                    IpAddress = ipHostInfo.AddressList[i];
                    break;
                }
            }
            if (IpAddress == null)
            {
                ThrowError(ErrorCode.NO_IPV4);
                return Task.CompletedTask;
            }
            
            // Begin listening
            Listener = new TcpListener(IpAddress, Port);
            Listener.Start();
            Listening = true;

            // Completed
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops updating and cleans up
        /// </summary>
        public Task Exit(bool ForceExit = false)
        {
            // Clean up
            Listening = false;
            if (ForceExit) CancellationSource.Cancel();

            // Completed
            return Task.CompletedTask;
        }

        /// <summary>
        /// Main listen loop
        /// </summary>
        private async Task Listen()
        {
            // Run listen loop
            while (Listening)
            {
                try
                {
                    TcpClient TcpClient = await Listener.AcceptTcpClientAsync();
                    await ReceiveConnectionAsync(TcpClient);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Begins the TCP listening loop
        /// </summary>
        public Task BeginUpdateAsync()
        {
            // Begin listening
            Listen();

            // Completed
            return Task.CompletedTask;
        }

        /// <summary>
        /// Connection handler
        /// </summary>
        private async Task ReceiveConnectionAsync(TcpClient TcpClient)
        {
            // Get connection endpoint
            string ClientEndPoint = TcpClient.Client.RemoteEndPoint.ToString();

            // Invoke connection event
            OnConnectionReceived?.Invoke(this, EventArgs.Empty);

            // Get packet data
            try
            {
                // Create variables
                NetworkStream networkStream = TcpClient.GetStream();
                StreamReader Reader = new StreamReader(networkStream);
                StreamWriter Writer = new StreamWriter(networkStream);
                Writer.AutoFlush = true;

                // Loop through packet data
                while (true)
                {
                    string Request = await Reader.ReadLineAsync();
                    if (Request != null)
                    {
                        // Invoke request received event
                        if (OnRequestReceived != null)
                            OnRequestReceived.Invoke(this, EventArgs.Empty);

                        // Process request
                        await ReceiveRequestAsync(Request, out string Response);

                        // Reply to request
                        await Writer.WriteLineAsync(Response);
                    }
                    else break; // Client closed connection
                }
                TcpClient.Close();
            }

            // Packet receive failed
            catch (Exception)
            {
                ThrowError(ErrorCode.PACKET_RECEIVE_ERROR);
                if (TcpClient.Connected) TcpClient.Close();
            }
        }

        /// <summary>
        /// Process requests sent to mobile server
        /// </summary>
        private Task ReceiveRequestAsync(string Request, out string Response)
        {
            // Process request here

            // Create response
            Response = "";

            // Completed
            return Task.CompletedTask;
        }
    }
}
