using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public abstract class UdpCommandInfo : ICommandInfo
    {
        public UdpCommandInfo(string key, string sessionID)
        {
            Key = key;
            SessionID = sessionID;
        }

        public string Key { get; private set; }

        public string SessionID { get; private set; }
    }
}
