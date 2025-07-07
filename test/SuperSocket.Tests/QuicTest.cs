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
            Assert.True(QuicListener.IsSupported, "QUIC is not supported.");
        }
    }
}