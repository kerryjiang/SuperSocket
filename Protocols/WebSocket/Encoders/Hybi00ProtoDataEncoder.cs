using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.Encoders
{
    class Hybi00ProtoDataEncoder : IProtoDataEncoder
    {
        private const string c_NotSupportErrorMessage = "The websocket of this version cannot used for sending binary data!";

        public IList<ArraySegment<byte>> EncodeData(IList<ArraySegment<byte>> data)
        {
            throw new NotSupportedException(c_NotSupportErrorMessage);
        }

        public IList<ArraySegment<byte>> EncodeData(ArraySegment<byte> data)
        {
            throw new NotSupportedException(c_NotSupportErrorMessage);
        }
    }
}
