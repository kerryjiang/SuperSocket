using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test
{
    public class TestRequestParser : IStringParser
    {
        public void Parse(string source, out string key, out string body, out string[] parameters)
        {
            key = string.Empty;
            body = string.Empty;
            parameters = null;

            int pos = source.IndexOf(':');

            if (pos <= 0)
                return;

            body = source.Substring(pos + 1);

            if (!string.IsNullOrEmpty(body))
                parameters = body.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
