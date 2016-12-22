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
        public override event EventHandler<ISocketAsyncEventArgs> Completed;

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public override ISocket AcceptSocket { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public override byte[] Buffer {
            get
            {
                return _socketAsyncEventArgs.Buffer;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public override IList<ArraySegment<byte>> BufferList
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
        public override int BytesTransferred
        {
            get
            {
                return _socketAsyncEventArgs.BytesTransferred;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public override Exception ConnectByNameError
        {
            get
            {
                return _socketAsyncEventArgs.ConnectByNameError;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public override ISocket ConnectSocket { get; set; }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public override int Count
        {
            get
            {
                return _socketAsyncEventArgs.Count;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public override bool DisconnectReuseSocket
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
        public override SocketAsyncOperation LastOperation
        {
            get
            {
                return _socketAsyncEventArgs.LastOperation;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public override int Offset
        {
            get
            {
                return _socketAsyncEventArgs.Offset;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public override IPPacketInformation ReceiveMessageFromPacketInfo
        {
            get
            {
                return _socketAsyncEventArgs.ReceiveMessageFromPacketInfo;
            }
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        public override EndPoint RemoteEndPoint
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
        public override SendPacketsElement[] SendPacketsElements
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
        public override TransmitFileOptions SendPacketsFlags
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
        public override int SendPacketsSendSize
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
        public override SocketError SocketError
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
        public override SocketFlags SocketFlags
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
        public override object UserToken
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
        public override void SetBuffer(byte[] buffer, int offset, int count)
        {
            _socketAsyncEventArgs.SetBuffer(buffer, offset, count);
        }

        /// <summary>
        /// See <see cref="SocketAsyncEventArgs"/>
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void SetBuffer(int offset, int count)
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
        public new void Dispose()
        {
            base.Dispose();
        }
    }
}