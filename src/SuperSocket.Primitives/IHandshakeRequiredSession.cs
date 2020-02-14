using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;

namespace SuperSocket
{
    public interface IHandshakeRequiredSession
    {
        bool Handshaked { get; }
    }
}