using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    public class UdpRequestInfo : IRequestInfo
    {
        public UdpRequestInfo(string key, string sessionID)
        {
            Key = key;
            SessionID = sessionID;
        }

        public string Key { get; private set; }

        public string SessionID { get; private set; }
    }
}
