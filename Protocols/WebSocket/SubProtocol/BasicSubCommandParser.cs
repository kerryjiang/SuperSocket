using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.WebSocket.SubProtocol
{
    /// <summary>
    /// Basic sub command parser
    /// </summary>
    public class BasicSubCommandParser : IRequestInfoParser<SubRequestInfo>
    {
        #region ISubProtocolCommandParser Members

        /// <summary>
        /// Parses the request info.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public SubRequestInfo ParseRequestInfo(string source)
        {
            var cmd = source.Trim();
            int pos = cmd.IndexOf(' ');
            string name;
            string param;

            if (pos > 0)
            {
                name = cmd.Substring(0, pos);
                param = cmd.Substring(pos + 1);
            }
            else
            {
                name = cmd;
                param = string.Empty;
            }

            pos = name.IndexOf('-');

            string token = string.Empty;

            if (pos > 0)
            {
                token = name.Substring(pos + 1);
                name = name.Substring(0, pos);
            }

            return new SubRequestInfo(name, token, param);
        }

        #endregion
    }
}
