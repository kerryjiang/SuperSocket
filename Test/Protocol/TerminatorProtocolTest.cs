using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Protocol
{
    [TestFixture]
    public class TerminatorProtocolTest : ProtocolTestBase
    {
        private readonly Encoding m_Encoding = new ASCIIEncoding();

        protected override IReceiveFilterFactory<StringPackageInfo> CurrentReceiveFilterFactory
        {
            get { return new TerminatorReceiveFilterFactory("##", m_Encoding); }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return string.Format("ECHO {0}##", sourceLine);
        }
    }
}
