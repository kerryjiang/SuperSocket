using System;
using System.Net.Quic;
using Xunit;

namespace SuperSocket.Tests
{
    public class QuicTest
    {
        [Fact(Skip = "QUIC requires .NET 9.0 SDK")]
        public void TestQuicSupport()
        {
#if NET9_0_OR_GREATER
            Assert.True(QuicListener.IsSupported, "QUIC is not supported.");
#else
            Assert.True(true, "QUIC test skipped - requires .NET 9.0 or greater");
#endif
        }
    }
}