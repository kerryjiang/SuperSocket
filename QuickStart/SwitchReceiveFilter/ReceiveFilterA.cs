using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase.Protocol;

namespace SwitchReceiveFilter
{
    class ReceiveFilterA : BeginEndMarkReceiveFilter<StringRequestInfo>
    {
        private static byte[] m_BeginMark = new byte[] { (byte)'Y' };
        private static byte[] m_EndMark = new byte[] { 0x00, 0xff };

        private static BasicRequestInfoParser m_Parser = new BasicRequestInfoParser();

        private SwitchReceiveFilter m_SwitchFilter;

        public ReceiveFilterA(SwitchReceiveFilter switcher)
            : base(m_BeginMark, m_EndMark)
        {
            m_SwitchFilter = switcher;
        }

        protected override StringRequestInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length)
        {
            var requestInfo = m_Parser.ParseRequestInfo(Encoding.ASCII.GetString(readBuffer, offset + 1, length - 3));
            NextReceiveFilter = m_SwitchFilter;
            return requestInfo;
        }
    }
}
