using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.CustomCommandParser
{
    /// <summary>
    /// CMD:ECHO AabSfght5656D5Cfa5==
    /// </summary>
    public class CustomStringParser : IStringParser
    {
        #region IStringParser Members

        public void Parse(string source, out string key, out string body, out string[] parameters)
        {
            if (!source.StartsWith("CMD:"))
                throw new ArgumentException("Unexpected source, the source should start with 'CMD:'.");

            source = source.Substring(4);
            string[] data = source.Split(' ');

            key = data[0];
            body = data[1];
            parameters = Encoding.ASCII.GetString(Convert.FromBase64String(data[1])).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion
    }
}
