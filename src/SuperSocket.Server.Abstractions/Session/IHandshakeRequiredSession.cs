using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions.Session
{
    public interface IHandshakeRequiredSession
    {
        bool Handshaked { get; }
    }
}