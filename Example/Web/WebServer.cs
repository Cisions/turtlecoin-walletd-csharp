using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TurtleCoinAPI
{
    public partial class WebServer
    {
        /// <summary>
        /// Initializes an http endpoint
        /// </summary>
        /// <param name="Port">Port to listen on</param>
        public Task InitializeAsync(Wallet Wallet, int Port = 80)
        {
            // Set local variables
            this.Wallet = Wallet;
            this.Port = Port;
            Endpoints = new Dictionary<string, Endpoint>();

            // Check that port is available
            if (TurtleCoin.Ping("localhost", Port))
            {
                ThrowError(ErrorCode.PORT_IN_USE);
                return Task.CompletedTask;
            }

            // Create listener object
            LogLine("Creating HTTP listener");
            Listener = new HttpListener();
            Initialized = true;

            // Completed
            return Task.CompletedTask;
        }

        public Task Add(Endpoint Endpoint)
        {
            // Make sure web server has been initialized
            if (!Initialized)
            {
                ThrowError(ErrorCode.NOT_INITIALIZED);
                return Task.CompletedTask;
            }

            // Check that response file exists
            if (!File.Exists(Endpoint.Response))
            {
                ThrowError(ErrorCode.INVALID_FILE);
                return Task.CompletedTask;
            }

            // Set response
            LogLine("Adding endpoint {0}", Endpoint.Name);
            Endpoint.Response = File.ReadAllText(Endpoint.Response);

            // Add endpoint to listener
            Listener.Prefixes.Add(string.Format("http://+:{0}/{1}/", Port, Endpoint.Name));
            Endpoints.Add(Endpoint.Name, Endpoint);

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
            Response = "";
            Listener.Stop();

            // Completed
            return Task.CompletedTask;
        }

        /// <summary>
        /// Main listen loop
        /// </summary>
        private async void Listen()
        {
            // Run listen loop
            while (Listening)
            {
                try
                {
                    // Grab connection context
                    var Context = await Listener.GetContextAsync();

                    // Handle request
                    ThreadPool.QueueUserWorkItem((o) =>
                    {
                        // Get listener context
                        var C = o as HttpListenerContext;
                        LogLine("Received request for {0}", C.Request.Url.AbsolutePath.Substring(1));

                        // Not a request
                        if (C.Request.Url.AbsolutePath.Substring(1) != "sendrequest")
                        {
                            // Get endpoint
                            Endpoint Endpoint = Endpoints[C.Request.Url.AbsolutePath.Substring(1)];

                            // Open file to send
                            Stream Input = new FileStream(Endpoint.RelativePath, FileMode.Open);

                            // Add headers
                            C.Response.ContentType = MIMEType.TryGetValue(Path.GetExtension(Endpoint.RelativePath), out string mime) ? mime : "application/octet-stream";
                            C.Response.ContentLength64 = Input.Length;
                            //C.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                            //C.Response.AddHeader("Last-Modified", File.GetLastWriteTime(Endpoint.RelativePath).ToString("r"));

                            // Write response to stream
                            byte[] Buffer;
                            if (!C.Response.ContentType.Contains("text"))
                            {
                                // Send file bytes
                                Buffer = new byte[1024 * 32];
                                int Bytes;
                                while ((Bytes = Input.Read(Buffer, 0, Buffer.Length)) > 0)
                                    C.Response.OutputStream.Write(Buffer, 0, Bytes);

                                // Close file stream
                                Input.Close();
                            }
                            else
                            {
                                using (StreamReader Reader = new StreamReader(Input))
                                {
                                    // Populate string then send its bytes
                                    string Output = PopulateResponse(Reader.ReadToEnd());
                                    Buffer = Encoding.UTF8.GetBytes(Output);
                                    C.Response.ContentLength64 = Buffer.Length;
                                    C.Response.OutputStream.Write(Buffer, 0, Buffer.Length);
                                }
                            }

                            // Close file stream
                            Input.Close();

                            // Write to stream
                            C.Response.OutputStream.Flush();
                            C.Response.StatusCode = (int)HttpStatusCode.OK;
                        }

                        // Request
                        else
                        {
                            using (StreamReader Reader = new StreamReader(C.Request.InputStream))
                            {
                                // Get request data
                                string InputData = Reader.ReadToEnd();
                                JObject Result = JObject.Parse(InputData);
                                Console.WriteLine(Result.ToString());

                                // Check that request is valid
                                if (!Result.ContainsKey("method") || !Result.ContainsKey("params") || !Result.ContainsKey("destination"))
                                    ThrowError(ErrorCode.BAD_REQUEST);

                                // Send request
                                else
                                {
                                    try
                                    {
                                        // Get method
                                        RequestMethod Method = new RequestMethod((string)Result["method"]);
                                        Console.WriteLine(Method);

                                        // Get parameters
                                        JObject Params = JObject.FromObject(Result["params"]);
                                        
                                        // Daemon request
                                        if (((string)Result["destination"]).ToLower() == "daemon")
                                            Daemon.SendRequestAsync(Method, Params, out Result);

                                        // Wallet request
                                        else if (((string)Result["destination"]).ToLower() == "wallet")
                                            Wallet.SendRequestAsync(Method, Params, out Result);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Error parsing params: {0}", e.Message);
                                        ThrowError(ErrorCode.BAD_REQUEST);
                                    }
                                }


                                // Set output string
                                string Output = Result.ToString();

                                // Add headers
                                C.Response.ContentType = "text/plain";
                                C.Response.ContentLength64 = Output.Length;

                                // Write response to stream
                                byte[] Buffer;

                                // Populate string then send its bytes
                                Buffer = Encoding.UTF8.GetBytes(Output);
                                C.Response.ContentLength64 = Buffer.Length;
                                C.Response.OutputStream.Write(Buffer, 0, Buffer.Length);

                                // Write to stream
                                C.Response.OutputStream.Flush();
                                C.Response.StatusCode = (int)HttpStatusCode.OK;
                            }
                        }
                    }, Context);
                }
                catch (Exception e)
                {
                    Console.WriteLine("STREAM ERROR: " + e.Message);
                }
            }
        }

        /// <summary>
        /// Begins the TCP listening loop
        /// </summary>
        public Task BeginUpdateAsync()
        {
            // Begin listening
            if (Initialized)
            {
                LogLine("Update started");
                Listener.Start();
                Listening = true;
                Listen();
            }
            else ThrowError(ErrorCode.NOT_INITIALIZED);

            // Completed
            return Task.CompletedTask;
        }
    }
}
