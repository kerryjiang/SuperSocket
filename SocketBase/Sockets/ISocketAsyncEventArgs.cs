using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SuperSocket.SocketBase.Sockets
{
    /// <summary>
    /// SocketAsyncEventArgs interface
    /// </summary>
    public abstract class ISocketAsyncEventArgs : EventArgs, IDisposable
    {
        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract event EventHandler<ISocketAsyncEventArgs> Completed;

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract ISocket AcceptSocket { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract byte[] Buffer { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract IList<ArraySegment<byte>> BufferList { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract int BytesTransferred { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract Exception ConnectByNameError { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract ISocket ConnectSocket { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract bool DisconnectReuseSocket { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract SocketAsyncOperation LastOperation { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract int Offset { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract IPPacketInformation ReceiveMessageFromPacketInfo { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract EndPoint RemoteEndPoint { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract SendPacketsElement[] SendPacketsElements { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract TransmitFileOptions SendPacketsFlags { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract int SendPacketsSendSize { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract SocketError SocketError { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract SocketFlags SocketFlags { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract object UserToken { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract void SetBuffer(byte[] buffer, int offset, int count);

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public abstract void SetBuffer(int offset, int count);

        /// <inheritdoc />
        public void Dispose() { }
    }
}