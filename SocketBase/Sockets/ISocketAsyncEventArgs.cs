using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SuperSocket.SocketBase.Sockets
{
    /// <summary>
    /// SocketAsyncEventArgs interface
    /// </summary>
    public interface ISocketAsyncEventArgs : IDisposable
    {
        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        event EventHandler<ISocketAsyncEventArgs> Completed;

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        ISocket AcceptSocket { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        byte[] Buffer { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        IList<ArraySegment<byte>> BufferList { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        int BytesTransferred { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        Exception ConnectByNameError { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        ISocket ConnectSocket { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        int Count { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        bool DisconnectReuseSocket { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        SocketAsyncOperation LastOperation { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        int Offset { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        IPPacketInformation ReceiveMessageFromPacketInfo { get; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        EndPoint RemoteEndPoint { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        SendPacketsElement[] SendPacketsElements { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        TransmitFileOptions SendPacketsFlags { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        int SendPacketsSendSize { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        SocketError SocketError { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        SocketFlags SocketFlags { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        object UserToken { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        void SetBuffer(byte[] buffer, int offset, int count);

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        void SetBuffer(int offset, int count);
    }
}