using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Protocol
{
    public class FixedSizeProtocolTest : ProtocolTestBase
    {
        class TestReceiveFilter : FixedSizeReceiveFilter<StringPackageInfo>
        {
            private BasicStringParser m_Parser = new BasicStringParser();

            public TestReceiveFilter()
                : base(41)
            {

            }

            public override StringPackageInfo ResolvePackage(IList<ArraySegment<byte>> packageData)
            {
                return new StringPackageInfo(Encoding.ASCII.GetString(packageData), m_Parser);
            }
        }

        protected override IReceiveFilterFactory<StringPackageInfo> CurrentReceiveFilterFactory
        {
            get { return new DefaultReceiveFilterFactory<TestReceiveFilter, StringPackageInfo>(); }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return "ECHO " + sourceLine;
        }
    }
}
