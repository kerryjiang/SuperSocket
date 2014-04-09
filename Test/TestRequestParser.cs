using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase.Command;
using SuperSocket.ProtoBase;

namespace SuperSocket.Test
{
    public class TestRequestParser : IStringPackageParser<StringRequestInfo>
    {
        #region ICommandParser Members

        public StringRequestInfo Parse(string source)
        {
            int pos = source.IndexOf(':');

            if(pos <= 0)
                return null;

            string param = source.Substring(pos + 1);

            return new StringRequestInfo(source.Substring(0, pos), param,
                param.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion
    }
}
