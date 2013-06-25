using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.Facility.Protocol;

namespace SwitchReceiveFilter
{
    class ReceiveFilterB : BeginEndMarkReceiveFilter<StringRequestInfo>
    {
        private static byte[] m_BeginMark = new byte[] { (byte)'*' };
        private static byte[] m_EndMark = new byte[] { (byte)'#' };

        private static BasicRequestInfoParser m_Parser = new BasicRequestInfoParser();

        private SwitchReceiveFilter m_SwitchFilter;

        public ReceiveFilterB(SwitchReceiveFilter switcher)
            : base(m_BeginMark, m_EndMark)
        {
            m_SwitchFilter = switcher;
        }

        protected override StringRequestInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length)
        {
            var requestInfo = m_Parser.ParseRequestInfo(Encoding.ASCII.GetString(readBuffer, offset + 1, length - 2));
            NextReceiveFilter = m_SwitchFilter;
            return requestInfo;
        }
    }
}
