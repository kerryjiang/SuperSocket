using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    public interface IHandshakeValidator
    {
        bool ValidateHandshake(ICommunicationChannel channel, HttpHeaderInfo handshakeRequest);
    }
}
