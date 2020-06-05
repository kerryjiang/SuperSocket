using System;
using System.Buffers;

namespace SuperSocket.WebSocket
{
    public interface IWebSocketExtension
    {
        void Encode(ref ReadOnlySequence<byte> data);

        void Decode(ref ReadOnlySequence<byte> data);
    }
}
