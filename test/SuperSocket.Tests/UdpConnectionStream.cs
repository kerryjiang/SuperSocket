using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using SuperSocket.Connection;

namespace SuperSocket.Tests
{
    public class UdpConnectionStream : Stream
    {
        public UdpPipeConnection Connection { get; }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new System.NotSupportedException();

        public override long Position { get => throw new System.NotSupportedException(); set => throw new System.NotSupportedException(); }

        public Socket Socket { get; }

        public UdpConnectionStream(UdpPipeConnection connection, Socket socket)
        {
            Connection = connection;
            Socket = socket;
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
            Connection
                .SendAsync((new ArraySegment<byte>(buffer, offset, count)).AsMemory(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        protected override void Dispose(bool disposing)
        {
            Socket?.Close();
            base.Dispose(disposing);
        }
    }
}