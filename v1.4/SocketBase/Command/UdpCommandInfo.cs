using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public abstract class UdpCommandInfo : ICommandInfo
    {
        public UdpCommandInfo(string key, string sessionKey)
        {
            Key = key;
            SessionKey = sessionKey;
        }

        public string Key { get; private set; }

        public string SessionKey { get; private set; }
    }
}
