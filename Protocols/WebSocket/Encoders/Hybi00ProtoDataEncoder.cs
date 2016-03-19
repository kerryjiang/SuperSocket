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

        public void EncodeData(IOutputBuffer output, IList<ArraySegment<byte>> data)
        {
            throw new NotSupportedException(c_NotSupportErrorMessage);
        }

        public void EncodeData(IOutputBuffer output, ArraySegment<byte> data)
        {
            throw new NotSupportedException(c_NotSupportErrorMessage);
        }
    }
}
