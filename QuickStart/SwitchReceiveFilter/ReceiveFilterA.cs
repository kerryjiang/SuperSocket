using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.QuickStart.SwitchReceiveFilter
{
    class ReceiveFilterA : BeginEndMarkReceiveFilter<StringPackageInfo>
    {
        private static byte[] m_BeginMark = new byte[] { (byte)'Y' };
        private static byte[] m_EndMark = new byte[] { 0x00, 0xff };

        private static IStringParser m_Parser = new BasicStringParser();

        private SwitchReceiveFilter m_SwitchFilter;

        public ReceiveFilterA(SwitchReceiveFilter switcher)
            : base(m_BeginMark, m_EndMark)
        {
            m_SwitchFilter = switcher;
        }

        public override StringPackageInfo ResolvePackage(IBufferStream bufferStream)
        {
            var requestInfo = new StringPackageInfo(bufferStream.Skip(1).ReadString((int)bufferStream.Length - 3, Encoding.ASCII), m_Parser);
            NextReceiveFilter = m_SwitchFilter;
            return requestInfo;
        }
    }
}
