using System;
using System.Collections.Generic;
using System.Net;

namespace TurtleCoinAPI
{
    public partial class WebServer
    {
        /// <summary>
        /// Throws an error
        /// </summary>
        /// <param name="e">Error code</param>
        /// <returns></returns>
        private void ThrowError(ErrorCode e)
        {
            Error?.Invoke(this, new TurtleCoinErrorEventArgs { Time = DateTime.Now, ErrorCode = e });
        }

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="Input">String to log</param>
        private void LogLine(string Input, params object[] args)
        {
            Log?.Invoke(this, new TurtleCoinLogEventArgs { Time = DateTime.Now, Message = String.Format(Input, args) });
        }

        /// <summary>
        /// Populates a response with wallet variables
        /// </summary>
        /// <param name="Input">Unpopulated response string</param>
        /// <returns>Response string populated with wallet variables</returns>
        private string PopulateResponse(string Input)
        {
            // Create an output string
            string Output = Input;

            // Add values
            try
            {
                Output = Output.Replace("{AVAILABLE_BALANCE}", Wallet.AvailableBalance.ToString());
                Output = Output.Replace("{LOCKED_AMOUNT}", Wallet.LockedAmount.ToString());
                Output = Output.Replace("{SYNCED}", Wallet.Synced.ToString());
                Output = Output.Replace("{BLOCK_COUNT}", Wallet.BlockCount.ToString());
                Output = Output.Replace("{KNOWN_BLOCK_COUNT}", Wallet.KnownBlockCount.ToString());
                Output = Output.Replace("{PEER_COUNT}", Wallet.PeerCount.ToString());
                Output = Output.Replace("{WALLET_FILE}", Wallet.File.ToString());
                Output = Output.Replace("{LAST_BLOCK_HASH}", Wallet.LastBlockHash.ToString());
                Output = Output.Replace("{LAST_BLOCK_HASH}", Wallet.LastBlockHash.ToString());
                Output = Output.Replace("{DAEMON_SYNCED}", Daemon.Synced.ToString());
                Output = Output.Replace("{WALLET_SYNCED}", Wallet.Synced.ToString());
            }
            catch { }

            // Completed
            return Output;
        }

        /// <summary>
        /// Converts file extension into mime type
        /// </summary>
        private static Dictionary<string, string> MIMEType =
            new Dictionary<string, string>
            {
                {".asf", "video/x-ms-asf"},
                {".asx", "video/x-ms-asf"},
                {".avi", "video/x-msvideo"},
                {".bin", "application/octet-stream"},
                {".cco", "application/x-cocoa"},
                {".crt", "application/x-x509-ca-cert"},
                {".css", "text/css"},
                {".deb", "application/octet-stream"},
                {".der", "application/x-x509-ca-cert"},
                {".dll", "application/octet-stream"},
                {".dmg", "application/octet-stream"},
                {".ear", "application/java-archive"},
                {".eot", "application/octet-stream"},
                {".exe", "application/octet-stream"},
                {".flv", "video/x-flv"},
                {".gif", "image/gif"},
                {".hqx", "application/mac-binhex40"},
                {".htc", "text/x-component"},
                {".htm", "text/html"},
                {".html", "text/html"},
                {".ico", "image/x-icon"},
                {".img", "application/octet-stream"},
                {".iso", "application/octet-stream"},
                {".jar", "application/java-archive"},
                {".jardiff", "application/x-java-archive-diff"},
                {".jng", "image/x-jng"},
                {".jnlp", "application/x-java-jnlp-file"},
                {".jpeg", "image/jpeg"},
                {".jpg", "image/jpeg"},
                {".js", "application/x-javascript"},
                {".json", "text/html"},
                {".mml", "text/mathml"},
                {".mng", "video/x-mng"},
                {".mov", "video/quicktime"},
                {".mp3", "audio/mpeg"},
                {".mpeg", "video/mpeg"},
                {".mpg", "video/mpeg"},
                {".msi", "application/octet-stream"},
                {".msm", "application/octet-stream"},
                {".msp", "application/octet-stream"},
                {".pdb", "application/x-pilot"},
                {".pdf", "application/pdf"},
                {".pem", "application/x-x509-ca-cert"},
                {".pl", "application/x-perl"},
                {".pm", "application/x-perl"},
                {".png", "image/png"},
                {".prc", "application/x-pilot"},
                {".ra", "audio/x-realaudio"},
                {".rar", "application/x-rar-compressed"},
                {".rpm", "application/x-redhat-package-manager"},
                {".rss", "text/xml"},
                {".run", "application/x-makeself"},
                {".sea", "application/x-sea"},
                {".shtml", "text/html"},
                {".sit", "application/x-stuffit"},
                {".swf", "application/x-shockwave-flash"},
                {".tcl", "application/x-tcl"},
                {".tk", "application/x-tcl"},
                {".txt", "text/plain"},
                {".war", "application/java-archive"},
                {".wbmp", "image/vnd.wap.wbmp"},
                {".wmv", "video/x-ms-wmv"},
                {".xml", "text/xml"},
                {".xpi", "application/x-xpinstall"},
                {".zip", "application/zip"}
            };

        /// <summary>
        /// Parses POST or GET data into a readable dictionary
        /// </summary>
        /// <param name="Input">Unparsed data</param>
        /// <returns>Dictionary of all non-empty parameters</returns>
        private static Dictionary<string, object> ParsePostGet(string Input)
        {
            string DataUrl = Input;
            string TempUrl;
            while ((TempUrl = Uri.UnescapeDataString(DataUrl)) != DataUrl)
                DataUrl = TempUrl;
            Dictionary<string, object> Output = new Dictionary<string, object>();
            foreach (var TokenValuePair in DataUrl.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var Tokens = TokenValuePair.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (Tokens.Length < 2)
                    continue;
                var paramName = Tokens[0];
                var paramValue = Tokens[1];
                var values = paramValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in values)
                {
                    var decodedValue = UrlDecode(value);
                    if (paramName.Contains("["))
                    {
                        string tempParamName = paramName.Substring(0, paramName.IndexOf('['));
                        if (!Output.ContainsKey(tempParamName))
                            Output.Add(tempParamName, new Dictionary<string, string>());
                        string subParamName = paramName.Split('[', ']')[1];
                        
                        if (subParamName.Contains("["))
                        {
                            string tempAgainParamName = subParamName.Substring(0, subParamName.IndexOf('['));
                            if (!Output.ContainsKey(tempAgainParamName))
                                Output.Add(tempAgainParamName, new Dictionary<string, string>());
                            string subAgainParamName = subParamName.Split('[', ']')[1];
                            if (!(Output[tempAgainParamName] as Dictionary<string, string>).ContainsKey(subAgainParamName))
                                (Output[tempAgainParamName] as Dictionary<string, string>).Add(subAgainParamName, decodedValue);
                        }
                        else if (!(Output[tempParamName] as Dictionary<string, string>).ContainsKey(subParamName))
                            (Output[tempParamName] as Dictionary<string, string>).Add(subParamName, decodedValue);
                    }
                    else Output.Add(paramName, decodedValue);
                }
            }
            return Output;
        }
        private static string UrlDecode(string Input)
        {
            string DataUrl = Input;
            string TempUrl;
            while ((TempUrl = Uri.UnescapeDataString(DataUrl)) != DataUrl)
                DataUrl = TempUrl;
            return DataUrl;
        }
    }

    public class Endpoint
    {
        public string Name;
        public string RelativePath;
        public string Response;
        public Endpoint(string Name, string Response)
        {
            this.Name = Name.Replace("\\", "/");
            this.Response = Response;
            RelativePath = Response;
        }
    }
}
