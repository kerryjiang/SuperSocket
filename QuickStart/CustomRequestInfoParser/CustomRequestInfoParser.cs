using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.ProtoBase;

namespace SuperSocket.QuickStart.CustomCommandParser
{
    /// <summary>
    /// CMD:ECHO AabSfght5656D5Cfa5==
    /// </summary>
    public class CustomRequestInfoParser : IStringPackageParser<StringRequestInfo>
    {
        #region ICommandParser Members

        public StringRequestInfo Parse(string source)
        {
            if(!source.StartsWith("CMD:"))
                return null;

            source = source.Substring(4);
            string[] data = source.Split(' ');
            return new StringRequestInfo(data[0], data[1],
                Encoding.ASCII.GetString(Convert.FromBase64String(data[1])).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion
    }
}
