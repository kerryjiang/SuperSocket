using System;
using System.Buffers;
using System.Net;

namespace SuperSocket.ProtoBase.ProxyProtocol
{
    public enum ProxyCommand
    {
        /// <summary>
        /// the connection was established on purpose by the proxy
        /// without being relayed. The connection endpoints are the sender and the
        /// receiver. Such connections exist when the proxy sends health-checks to the
        /// server. The receiver must accept this connection as valid and must use the
        /// real connection endpoints and discard the protocol block including the
        /// family which is ignored.
        /// </summary>
        LOCAL,

        /// <summary>
        /// the connection was established on behalf of another node,
        /// and reflects the original connection endpoints. The receiver must then use
        /// the information provided in the protocol block to get original the address.
        /// </summary>
        PROXY
    }
}