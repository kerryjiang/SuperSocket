using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Protocol
{
    class NewLineProtoTextEncoder : IProtoTextEncoder
    {
        private static string s_NewLine = "\r\n";

        private Encoding m_Encoding;

        public NewLineProtoTextEncoder(Encoding encoding)
        {
            m_Encoding = encoding;
        }

        public IList<ArraySegment<byte>> EncodeText(string message)
        {
            return new ArraySegment<byte>[]
                {
                    new ArraySegment<byte>(m_Encoding.GetBytes(message + s_NewLine))
                };
        }
    }
}
