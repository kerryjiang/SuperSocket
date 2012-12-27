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
    public class CountSpliterProtocolTest : ProtocolTestBase
    {
        class TestReceiveFilter : CountSpliterReceiveFilter<StringRequestInfo>
        {
            public TestReceiveFilter()
                : base((byte)'#', 3)
            {

            }

            protected override StringRequestInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length)
            {
                var line = Encoding.ASCII.GetString(readBuffer, offset, length);
                var arr = line.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                return new StringRequestInfo(arr[0], arr[1], new string[] { arr[1] });
            }
        }

        protected override IReceiveFilterFactory<StringRequestInfo> CurrentReceiveFilterFactory
        {
            get { return new DefaultReceiveFilterFactory<TestReceiveFilter, StringRequestInfo>(); }
        }

        protected override string CreateRequest(string sourceLine)
        {
            return string.Format("#ECHO#{0}#", sourceLine);
        }
    }
}
