using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.Encoders
{
    class Rfc6455ProtoTextEncoder : Rfc6455ProtoDataEncoder, IProtoTextEncoder
    {
        private readonly static Encoding s_Encoding = new UTF8Encoding();

        public IList<ArraySegment<byte>> EncodeText(string message)
        {
            return new ArraySegment<byte>[]
                {
                    EncodeData((int)OpCode.Binary, true, new ArraySegment<byte>(s_Encoding.GetBytes(message)))
                };
        }
    }
}
