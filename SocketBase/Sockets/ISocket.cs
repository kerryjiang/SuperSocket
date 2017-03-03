using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace SuperSocket.SocketBase.Sockets
{
    /// <summary>
    /// Socket interface
    /// </summary>
    public interface ISocket
    {
        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        bool ExclusiveAddressUse { get; set; }

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        EndPoint LocalEndPoint { get; }

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        bool NoDelay { get; set; }

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        int ReceiveBufferSize { get; set; }

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        int SendBufferSize { get; set; }

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        int SendTimeout { get; set; }

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="e"></param>
        bool AcceptAsync(ISocketAsyncEventArgs e);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="remoteEP"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        IAsyncResult BeginConnect(EndPoint remoteEP, AsyncCallback callback, object state);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="localEP"></param>
        void Bind(EndPoint localEP);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        void Close();

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="timeout"></param>
        void Close(int timeout);

        /// <summary>
        /// Creates a new stream
        /// </summary>
        /// <returns></returns>
        Stream CreateStream();

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="asyncResult"></param>
        void EndConnect(IAsyncResult asyncResult);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="ioControlCode"></param>
        /// <param name="optionInValue"></param>
        /// <param name="optionOutValue"></param>
        int IOControl(int ioControlCode, byte[] optionInValue, byte[] optionOutValue);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="ioControlCode"></param>
        /// <param name="optionInValue"></param>
        /// <param name="optionOutValue"></param>
        int IOControl(IOControlCode ioControlCode, byte[] optionInValue, byte[] optionOutValue);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="backlog"></param>
        void Listen(int backlog);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="e"></param>
        bool ReceiveAsync(ISocketAsyncEventArgs e);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="e"></param>
        bool ReceiveFromAsync(ISocketAsyncEventArgs e);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="buffer"></param>
        int Send(byte[] buffer);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="socketFlags"></param>
        int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="socketFlags"></param>
        /// <param name="errorCode"></param>
        int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="size"></param>
        /// <param name="socketFlags"></param>
        int Send(byte[] buffer, int size, SocketFlags socketFlags);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="socketFlags"></param>
        int Send(byte[] buffer, SocketFlags socketFlags);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="buffers"></param>
        int Send(IList<ArraySegment<byte>> buffers);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="buffers"></param>
        /// <param name="socketFlags"></param>
        int Send(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="buffers"></param>
        /// <param name="socketFlags"></param>
        /// <param name="socketError"></param>
        int Send(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError socketError);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="e"></param>
        bool SendAsync(ISocketAsyncEventArgs e);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="socketFlags"></param>
        /// <param name="remoteEP"></param>
        int SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="e"></param>
        bool SendToAsync(ISocketAsyncEventArgs e);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="optionLevel"></param>
        /// <param name="optionName"></param>
        /// <param name="optionValue"></param>
        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="optionLevel"></param>
        /// <param name="optionName"></param>
        /// <param name="optionValue"></param>
        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="optionLevel"></param>
        /// <param name="optionName"></param>
        /// <param name="optionValue"></param>
        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="optionLevel"></param>
        /// <param name="optionName"></param>
        /// <param name="optionValue"></param>
        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue);

        /// <summary>
        /// See <see cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="socketShutdown"></param>
        void Shutdown(SocketShutdown socketShutdown);
    }
}