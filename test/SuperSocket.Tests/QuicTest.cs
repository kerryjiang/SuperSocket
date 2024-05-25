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
#pragma warning disable CA2252,CA1416
            Assert.True(QuicListener.IsSupported, "QUIC is not supported.");
#pragma warning restore CA2252,CA1416
        }
    }
}