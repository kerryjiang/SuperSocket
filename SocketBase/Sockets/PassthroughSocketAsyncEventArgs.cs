using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SuperSocket.SocketBase.Sockets
{
    /// <summary>
    /// Passthrough SocketAsyncEventArgs
    /// </summary>
    public class PassthroughSocketAsyncEventArgs : ISocketAsyncEventArgs
    {
        private readonly SocketAsyncEventArgs _socketAsyncEventArgs;

        /// <summary>
        /// SocketAsyncEventArgs
        /// </summary>
        public SocketAsyncEventArgs SocketAsyncEventArgs
        {
            get
            {
                return _socketAsyncEventArgs;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public event EventHandler<ISocketAsyncEventArgs> Completed;

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public ISocket AcceptSocket { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public byte[] Buffer {
            get
            {
                return _socketAsyncEventArgs.Buffer;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public IList<ArraySegment<byte>> BufferList
        {
            get
            {
                return _socketAsyncEventArgs.BufferList;
            }

            set
            {
                _socketAsyncEventArgs.BufferList = value;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public int BytesTransferred
        {
            get
            {
                return _socketAsyncEventArgs.BytesTransferred;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public Exception ConnectByNameError
        {
            get
            {
                return _socketAsyncEventArgs.ConnectByNameError;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public ISocket ConnectSocket { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public int Count
        {
            get
            {
                return _socketAsyncEventArgs.Count;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public bool DisconnectReuseSocket
        {
            get
            {
                return _socketAsyncEventArgs.DisconnectReuseSocket;
            }

            set
            {
                _socketAsyncEventArgs.DisconnectReuseSocket = value;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public SocketAsyncOperation LastOperation
        {
            get
            {
                return _socketAsyncEventArgs.LastOperation;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public int Offset
        {
            get
            {
                return _socketAsyncEventArgs.Offset;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public IPPacketInformation ReceiveMessageFromPacketInfo
        {
            get
            {
                return _socketAsyncEventArgs.ReceiveMessageFromPacketInfo;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get
            {
                return _socketAsyncEventArgs.RemoteEndPoint;
            }

            set
            {
                _socketAsyncEventArgs.RemoteEndPoint = value;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public SendPacketsElement[] SendPacketsElements
        {
            get
            {
                return _socketAsyncEventArgs.SendPacketsElements;
            }

            set
            {
                _socketAsyncEventArgs.SendPacketsElements = value;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public TransmitFileOptions SendPacketsFlags
        {
            get
            {
                return _socketAsyncEventArgs.SendPacketsFlags;
            }

            set
            {
                _socketAsyncEventArgs.SendPacketsFlags = value;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public int SendPacketsSendSize
        {
            get
            {
                return _socketAsyncEventArgs.SendPacketsSendSize;
            }

            set
            {
                _socketAsyncEventArgs.SendPacketsSendSize = value;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public SocketError SocketError
        {
            get
            {
                return _socketAsyncEventArgs.SocketError;
            }

            set
            {
                _socketAsyncEventArgs.SocketError = value;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public SocketFlags SocketFlags
        {
            get
            {
                return _socketAsyncEventArgs.SocketFlags;
            }

            set
            {
                _socketAsyncEventArgs.SocketFlags = value;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public object UserToken
        {
            get
            {
                return _socketAsyncEventArgs.UserToken;
            }

            set
            {
                _socketAsyncEventArgs.UserToken = value;
            }
        }

        /// <summary>
        /// Instantiates a <see cref="SocketAsyncEventArgs"/> adapter
        /// </summary>
        public PassthroughSocketAsyncEventArgs()
        {
            _socketAsyncEventArgs = new SocketAsyncEventArgs();
            _socketAsyncEventArgs.Completed += (sender, args) =>
            {
                this.Map(args);

                this.OnPassthroughSocketAsyncEventArgsCompleted();
            };
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void SetBuffer(byte[] buffer, int offset, int count)
        {
            _socketAsyncEventArgs.SetBuffer(buffer, offset, count);
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public void SetBuffer(int offset, int count)
        {
            _socketAsyncEventArgs.SetBuffer(offset, count);
        }

        /// <summary>
        /// Maps a <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        /// <param name="e"></param>
        public void Map(SocketAsyncEventArgs e)
        {
            this.AcceptSocket = new PassthroughSocket(e.AcceptSocket);
            this.ConnectSocket = new PassthroughSocket(e.ConnectSocket);
        }

        /// <summary>
        /// Fires Completed event
        /// </summary>
        public void OnPassthroughSocketAsyncEventArgsCompleted()
        {
            var handler = Completed;
            if (null != handler)
            {
                handler(this, this);
            }
        }

        /// <inheritdoc />
        public void Dispose() { }
    }
}