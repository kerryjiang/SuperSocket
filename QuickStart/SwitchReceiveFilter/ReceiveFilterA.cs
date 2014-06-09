using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.ProtoBase;

namespace SuperSocket.QuickStart.SwitchReceiveFilter
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

        public override StringRequestInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
        {
            using (var reader = this.GetBufferReader(packageData))
            {
                var requestInfo = m_Parser.Parse(reader.Skip(1).ReadString((int)reader.Length - 3, Encoding.ASCII));
                NextReceiveFilter = m_SwitchFilter;
                return requestInfo;
            }
        }
    }
}
