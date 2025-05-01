using System;
using System.Net.Quic;
using Xunit;

namespace SuperSocket.Tests
{
    public class QuicTest
    {
        [Fact]
        public void TestQuicSupport()
        {
            Assert.True(QuicListener.IsSupported, "QUIC is not supported.");
        }
    }
}