using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket;
using SuperSocket.Client;
using SuperSocket.ProtoBase;
using SuperSocket.Udp;

namespace SuperSocket.Tests
{
    public class UdpChannelStream : Stream
    {
        public UdpPipeChannel<TextPackageInfo> Channel { get; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new System.NotSupportedException();

        public override long Position { get => throw new System.NotSupportedException(); set => throw new System.NotSupportedException(); }

        public UdpChannelStream(UdpPipeChannel<TextPackageInfo> channel)
        {
            Channel = channel;
        }

        public override void Flush()
        {
            
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Channel
                .WritePipeDataAsync((new ArraySegment<byte>(buffer, offset, count)).AsMemory(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }
    }
}