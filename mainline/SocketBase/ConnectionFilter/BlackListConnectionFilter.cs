using System;
using System.Collections.Specialized;
using System.Net;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.ConnectionFilter
{
    public class BlackListConnectionFilter : IConnectionFilter
    {
        #region IConnectionFilter implementation
        
        public bool Initialize(string name, NameValueCollection options)
        {
            Name = name;
            return true;
        }

        public bool AllowConnect(IPEndPoint remoteAddress)
        {
            throw new NotImplementedException();
        }

        public string Name { get; private set; }
        
        #endregion
    }
}

