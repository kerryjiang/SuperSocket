using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    public class RequestInfo<TRequestData> : IRequestInfo<TRequestData>
    {
        protected RequestInfo(string key, TRequestData data)
        {
            Key = key;
            Data = data;
        }

        public string Key { get; private set; }

        public TRequestData Data { get; private set; }
    }
}
