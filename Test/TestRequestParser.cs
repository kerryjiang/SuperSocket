using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Test
{
    public class TestRequestParser : IStringPackageParser<StringPackageInfo>
    {
        #region ICommandParser Members

        public StringPackageInfo Parse(string source)
        {
            int pos = source.IndexOf(':');

            if(pos <= 0)
                return null;

            string param = source.Substring(pos + 1);

            return new StringPackageInfo(source.Substring(0, pos), param,
                param.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries));
        }

        #endregion
    }
}
