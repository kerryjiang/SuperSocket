using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Protocol
{
    [TestFixture]
    public class FixedHeaderProtocolTest : ProtocolTestBase
    {
        class TestReceiveFilter : FixedHeaderReceiveFilter<StringRequestInfo>
        {
            public TestReceiveFilter()
                : base(8)
            {

            }

            protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
            {
                var strLen = Encoding.ASCII.GetString(header, offset + 4, 4);
                return int.Parse(strLen.TrimStart('0'));
            }

            protected override StringRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
            {
                var body = Encoding.ASCII.GetString(bodyBuffer, offset, length);
                return new StringRequestInfo(Encoding.ASCII.GetString(header.Array, header.Offset, 4), body, new string[] { body });
            }
        }

        protected override IReceiveFilterFactory<StringRequestInfo> CurrentReceiveFilterFactory
        {
            get { return new DefaultReceiveFilterFactory<TestReceiveFilter, StringRequestInfo>(); }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return string.Format("ECHO{0}{1}", sourceLine.Length.ToString().PadLeft(4), sourceLine);
        }
    }
}
