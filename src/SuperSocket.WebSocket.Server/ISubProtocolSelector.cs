using System;
using System.Net;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket.Server
{
    public interface ISubProtocolSelector
    {
        ValueTask<string> Select(string[] subProtocols, HttpHeader handshakeRequest);
    }
}