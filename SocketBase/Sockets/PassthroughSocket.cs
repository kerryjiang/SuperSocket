using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SuperSocket.SocketBase.Sockets
{
    /// <summary>
    /// Passthrough Socket
    /// </summary>
    public class PassthroughSocket : ISocket
    {
        private readonly Socket _socket;
        
        /// <inheritdoc />
        public bool Connected
        {
            get
            {
                return _socket.Connected;
            }
        }
        
        /// <inheritdoc />
        public bool ExclusiveAddressUse
        {
            get
            {
                return _socket.ExclusiveAddressUse;
            }

            set
            {
                _socket.ExclusiveAddressUse = value;
            }
        }
        
        /// <inheritdoc />
        public EndPoint LocalEndPoint
        {
            get
            {
                return _socket.LocalEndPoint;
            }
        }

        /// <inheritdoc />
        public bool NoDelay
        {
            get
            {
                return _socket.NoDelay;
            }

            set
            {
                _socket.NoDelay = value;
            }
        }

        /// <inheritdoc />
        public int ReceiveBufferSize
        {
            get
            {
                return _socket.ReceiveBufferSize;
            }

            set
            {
                _socket.ReceiveBufferSize = value;
            }
        }

        /// <inheritdoc />
        public EndPoint RemoteEndPoint
        {
            get
            {
                return _socket.RemoteEndPoint;
            }
        }

        /// <inheritdoc />
        public int SendBufferSize
        {
            get
            {
                return _socket.SendBufferSize;
            }

            set
            {
                _socket.SendBufferSize = value;
            }
        }

        /// <inheritdoc />
        public int SendTimeout
        {
            get
            {
                return _socket.SendTimeout;
            }

            set
            {
                _socket.SendTimeout = value;
            }
        }

        /// <summary>
        /// Instantiates a <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="socket"></param>
        public PassthroughSocket(Socket socket)
        {
            _socket = socket;
        }

        /// <summary>
        /// Instantiates a <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="socketInformation"></param>
        public PassthroughSocket(SocketInformation socketInformation)
        {
            _socket = new Socket(socketInformation);
        }

        /// <summary>
        /// Instantiates a <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="addressFamily"></param>
        /// <param name="socketType"></param>
        /// <param name="protocolType"></param>
        public PassthroughSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            _socket = new Socket(addressFamily, socketType, protocolType);
        }

        /// <summary>
        /// Instantiates a <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="socketType"></param>
        /// <param name="protocolType"></param>
        public PassthroughSocket(SocketType socketType, ProtocolType protocolType)
        {
            _socket = new Socket(socketType, protocolType);
        }

        /// <inheritdoc />
        public bool AcceptAsync(ISocketAsyncEventArgs e)
        {
            return _socket.AcceptAsync(((PassthroughSocketAsyncEventArgs)e).SocketAsyncEventArgs);
        }

        /// <inheritdoc />
        public IAsyncResult BeginConnect(EndPoint remoteEP, AsyncCallback callback, object state)
        {
            return _socket.BeginConnect(remoteEP, callback, state);
        }

        /// <inheritdoc />
        public void Bind(EndPoint localEP)
        {
            _socket.Bind(localEP);
        }

        /// <inheritdoc />
        public void Close()
        {
            _socket.Close();
        }

        /// <inheritdoc />
        public void Close(int timeout)
        {
            _socket.Close(timeout);
        }

        /// <inheritdoc />
        public Stream CreateStream()
        {
            return new NetworkStream(_socket);
        }

        /// <inheritdoc />
        public void EndConnect(IAsyncResult asyncResult)
        {
            _socket.EndConnect(asyncResult);
        }

        /// <inheritdoc />
        public int IOControl(int ioControlCode, byte[] optionInValue, byte[] optionOutValue)
        {
            return _socket.IOControl(ioControlCode, optionInValue, optionOutValue);
        }

        /// <inheritdoc />
        public int IOControl(IOControlCode ioControlCode, byte[] optionInValue, byte[] optionOutValue)
        {
            return _socket.IOControl(ioControlCode, optionInValue, optionOutValue);
        }

        /// <inheritdoc />
        public void Listen(int backlog)
        {
            _socket.Listen(backlog);
        }

        /// <inheritdoc />
        public bool ReceiveAsync(ISocketAsyncEventArgs e)
        {
            return _socket.ReceiveAsync(((PassthroughSocketAsyncEventArgs)e).SocketAsyncEventArgs);
        }

        /// <inheritdoc />
        public bool ReceiveFromAsync(ISocketAsyncEventArgs e)
        {
            return _socket.ReceiveFromAsync(((PassthroughSocketAsyncEventArgs)e).SocketAsyncEventArgs);
        }

        /// <inheritdoc />
        public int Send(byte[] buffer)
        {
            return _socket.Send(buffer);
        }

        /// <inheritdoc />
        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            return _socket.Send(buffer, offset, size, socketFlags);
        }

        /// <inheritdoc />
        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode)
        {
            return _socket.Send(buffer, offset, size, socketFlags, out errorCode);
        }

        /// <inheritdoc />
        public int Send(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return _socket.Send(buffer, size, socketFlags);
        }

        /// <inheritdoc />
        public int Send(byte[] buffer, SocketFlags socketFlags)
        {
            return _socket.Send(buffer, socketFlags);
        }

        /// <inheritdoc />
        public int Send(IList<ArraySegment<byte>> buffers)
        {
            return _socket.Send(buffers);
        }

        /// <inheritdoc />
        public int Send(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
        {
            return _socket.Send(buffers, socketFlags);
        }

        /// <inheritdoc />
        public int Send(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError socketError)
        {
            return _socket.Send(buffers, socketFlags, out socketError);
        }

        /// <inheritdoc />
        public bool SendAsync(ISocketAsyncEventArgs e)
        {
            return _socket.SendAsync(((PassthroughSocketAsyncEventArgs)e).SocketAsyncEventArgs);
        }

        /// <inheritdoc />
        public int SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP)
        {
            return _socket.SendTo(buffer, offset, size, socketFlags, remoteEP);
        }

        /// <inheritdoc />
        public bool SendToAsync(ISocketAsyncEventArgs e)
        {
            return _socket.SendToAsync(((PassthroughSocketAsyncEventArgs)e).SocketAsyncEventArgs);
        }

        /// <inheritdoc />
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
        {
            _socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        /// <inheritdoc />
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
        {
            _socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        /// <inheritdoc />
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
        {
            _socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        /// <inheritdoc />
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
        {
            _socket.SetSocketOption(optionLevel, optionName, optionValue);
        }

        /// <inheritdoc />
        public void Shutdown(SocketShutdown socketShutdown)
        {
            _socket.Shutdown(socketShutdown);
        }
    }
}