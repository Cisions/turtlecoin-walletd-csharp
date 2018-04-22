using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TurtleCoinAPI
{
    public partial class Mobile
    {
        /// <summary>
        /// Throws an error
        /// </summary>
        /// <param name="e">Error code</param>
        /// <returns></returns>
        private void ThrowError(ErrorCode e)
        {
            Error?.Invoke(this, new ErrorEventArgs { Time = DateTime.Now, ErrorCode = e });
        }

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="Input">String to log</param>
        private void LogLine(string Input, params object[] args)
        {
            Log?.Invoke(this, new LogEventArgs { Time = DateTime.Now, Message = String.Format(Input, args) });
        }
    }
}
