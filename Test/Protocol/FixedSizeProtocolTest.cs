using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.ProtoBase;

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

            public override StringRequestInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
            {
                return m_Parser.Parse(Encoding.ASCII.GetString(packageData));
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
