using System;
using System.Buffers;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions.Session;
using ConnectionCloseReason = SuperSocket.Connection.CloseReason;

namespace SuperSocket.WebSocket.Server
{
    /// <summary>
    /// Represents a WebSocket session with methods for sending and closing connections.
    /// </summary>
    public class WebSocketSession : AppSession, IHandshakeRequiredSession
    {
        /// <summary>
        /// Gets or sets a value indicating whether the handshake is completed.
        /// </summary>
        public bool Handshaked { get; internal set; }

        /// <summary>
        /// Gets the HTTP header associated with the WebSocket session.
        /// </summary>
        public HttpHeader HttpHeader { get; internal set; }

        /// <summary>
        /// Gets the path of the WebSocket session.
        /// </summary>
        public string Path
        {
            get { return HttpHeader.Path; }
        }

        /// <summary>
        /// Gets or sets the sub-protocol used in the WebSocket session.
        /// </summary>
        public string SubProtocol { get; internal set; }

        internal ISubProtocolHandler SubProtocolHandler { get; set; }

        /// <summary>
        /// Gets the time when the close handshake started.
        /// </summary>
        public DateTime CloseHandshakeStartTime { get; private set; }

        /// <summary>
        /// Occurs when the close handshake starts.
        /// </summary>
        public event EventHandler CloseHandshakeStarted;

        internal CloseStatus CloseStatus { get; set; }        

        internal IPackageEncoder<WebSocketPackage> MessageEncoder { get; set; }

        /// <summary>
        /// Sends a WebSocket package asynchronously.
        /// </summary>
        /// <param name="message">The WebSocket package to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public virtual ValueTask SendAsync(WebSocketPackage message, CancellationToken cancellationToken = default)
        {
            return this.Connection.SendAsync(MessageEncoder, message, cancellationToken);
        }

        /// <summary>
        /// Sends a text message asynchronously.
        /// </summary>
        /// <param name="message">The text message to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public virtual ValueTask SendAsync(string message, CancellationToken cancellationToken = default)
        {
            return SendAsync(new WebSocketPackage
                {
                    OpCode = OpCode.Text,
                    Message = message,
                },
                cancellationToken);
        }

        /// <summary>
        /// Sends binary data asynchronously.
        /// </summary>
        /// <param name="data">The binary data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public virtual ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            return SendAsync(new WebSocketPackage
                {
                    OpCode = OpCode.Binary,
                    Data = new ReadOnlySequence<byte>(data),
                },
                cancellationToken);
        }

        /// <summary>
        /// Sends binary data asynchronously.
        /// </summary>
        /// <param name="data">The binary data to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public virtual ValueTask SendAsync(ReadOnlySequence<byte> data, CancellationToken cancellationToken = default)
        {
            return SendAsync(new WebSocketPackage
                {
                    OpCode = OpCode.Binary,
                    Data = data,
                },
                cancellationToken);
        }

        /// <summary>
        /// Closes the WebSocket session asynchronously with the specified reason.
        /// </summary>
        /// <param name="reason">The reason for closing the session.</param>
        /// <param name="reasonText">The reason text.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous close operation.</returns>
        public ValueTask CloseAsync(CloseReason reason, string reasonText = null, CancellationToken cancellationToken = default)
        {
            var closeReasonCode = (short)reason;

            var closeStatus = new CloseStatus
            {
                Reason = reason
            };

            var textEncodedLen = 0;

            if (!string.IsNullOrEmpty(reasonText))
                textEncodedLen = Encoding.UTF8.GetMaxByteCount(reasonText.Length);

            var buffer = new byte[textEncodedLen + 2];

            buffer[0] = (byte) (closeReasonCode / 256);
            buffer[1] = (byte) (closeReasonCode % 256);

            var length = 2;

            if (!string.IsNullOrEmpty(reasonText))
            {
                closeStatus.ReasonText = reasonText;
                var span = new Span<byte>(buffer, 2, buffer.Length - 2);
                length += Encoding.UTF8.GetBytes(reasonText.AsSpan(), span);
            }

            CloseStatus = closeStatus;

            CloseHandshakeStartTime = DateTime.Now;
            OnCloseHandshakeStarted();

            return SendAsync(new WebSocketPackage
                {
                    OpCode = OpCode.Close,
                    Data = new ReadOnlySequence<byte>(buffer, 0, length)
                },
                cancellationToken);
        }

        private void OnCloseHandshakeStarted()
        {
            CloseHandshakeStarted?.Invoke(this, EventArgs.Empty);
        }

        internal void CloseWithoutHandshake()
        {
            base.CloseAsync(ConnectionCloseReason.LocalClosing).DoNotAwait();
        }

        /// <summary>
        /// Closes the WebSocket session asynchronously.
        /// </summary>
        /// <param name="closeReason">The reason for closing the connection.</param>
        /// <returns>A task that represents the asynchronous close operation.</returns>
        public override async ValueTask CloseAsync(ConnectionCloseReason closeReason)
        {
            var closeStatus = CloseStatus;

            if (closeStatus != null)
            {
                var clientInitiated = closeStatus.RemoteInitiated;
                await base.CloseAsync(clientInitiated ? ConnectionCloseReason.RemoteClosing : ConnectionCloseReason.LocalClosing);
                return;
            }

            try
            {
                await CloseAsync(CloseReason.NormalClosure);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Closes the WebSocket session asynchronously with a normal closure reason.
        /// </summary>
        /// <returns>A task that represents the asynchronous close operation.</returns>
        public override async ValueTask CloseAsync()
        {
            await this.CloseAsync(CloseReason.NormalClosure);
        }
    }
}