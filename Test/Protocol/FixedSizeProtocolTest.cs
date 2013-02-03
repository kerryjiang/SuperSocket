using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Facility.Protocol;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Protocol
{
    public class FixedSizeProtocolTest : ProtocolTestBase
    {
        class TestReceiveFilter : FixedSizeReceiveFilter<StringRequestInfo>
        {
            private BasicRequestInfoParser m_Parser = new BasicRequestInfoParser();

            public TestReceiveFilter()
                : base(41)
            {

            }

            protected override StringRequestInfo ProcessMatchedRequest(byte[] buffer, int offset, int length, bool toBeCopied)
            {
                return m_Parser.ParseRequestInfo(Encoding.ASCII.GetString(buffer, offset, length));
            }
        }

        protected override IReceiveFilterFactory<StringRequestInfo> CurrentReceiveFilterFactory
        {
            get { return new DefaultReceiveFilterFactory<TestReceiveFilter, StringRequestInfo>(); }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return "ECHO " + sourceLine;
        }
    }
}
