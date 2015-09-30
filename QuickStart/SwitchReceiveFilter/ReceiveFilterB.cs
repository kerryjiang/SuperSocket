using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.QuickStart.SwitchReceiveFilter
{
    class ReceiveFilterB : BeginEndMarkReceiveFilter<StringPackageInfo>
    {
        private static byte[] m_BeginMark = new byte[] { (byte)'*' };
        private static byte[] m_EndMark = new byte[] { (byte)'#' };

        private static IStringParser m_Parser = new BasicStringParser();

        private SwitchReceiveFilter m_SwitchFilter;

        public ReceiveFilterB(SwitchReceiveFilter switcher)
            : base(m_BeginMark, m_EndMark)
        {
            m_SwitchFilter = switcher;
        }

        public override StringPackageInfo ResolvePackage(IBufferStream bufferStream)
        {
            var requestInfo = new StringPackageInfo(bufferStream.Skip(1).ReadString((int)bufferStream.Length - 2, Encoding.ASCII), m_Parser);
            NextReceiveFilter = m_SwitchFilter;
            return requestInfo;
        }
    }
}
