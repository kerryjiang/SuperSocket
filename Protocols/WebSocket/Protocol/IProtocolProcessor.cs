using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.WebSocket.Protocol
{
    /// <summary>
    /// Protocol processor interface
    /// </summary>
    public interface IProtocolProcessor
    {
        /// <summary>
        /// Gets a value indicating whether this instance can send binary data.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance can send binary data; otherwise, <c>false</c>.
        /// </value>
        bool CanSendBinaryData { get; }

        /// <summary>
        /// Gets the close status clode.
        /// </summary>
        ICloseStatusCode CloseStatusClode { get; }

        /// <summary>
        /// Gets or sets the next processor.
        /// </summary>
        /// <value>
        /// The next processor.
        /// </value>
        IProtocolProcessor NextProcessor { get; set; }

        /// <summary>
        /// Handshakes the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="previousFilter">The previous filter.</param>
        /// <param name="dataFrameReader">The data frame reader.</param>
        /// <returns></returns>
        bool Handshake(IWebSocketSession session, WebSocketReceiveFilterBase previousFilter, out IReceiveFilter<IWebSocketFragment> dataFrameReader);

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">The message.</param>
        void SendMessage(IWebSocketSession session, string message);

        /// <summary>
        /// Try to send the message.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="message">The message.</param>
        /// <returns>if the messaged has been enqueued into the sending queue, return true; else if the message failed to be enqueued becuase the sending is full, then return false</returns>
        bool TrySendMessage(IWebSocketSession session, string message);

        /// <summary>
        /// Sends the data.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        void SendData(IWebSocketSession session, byte[] data, int offset, int length);

        /// <summary>
        /// Try to send the data.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>if the data has been enqueued into the sending queue, return true; else if the data failed to be enqueued becuase the sending is full, then return false</returns>
        bool TrySendData(IWebSocketSession session, byte[] data, int offset, int length);

        /// <summary>
        /// Sends the close handshake.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="closeReason">The close reason.</param>
        void SendCloseHandshake(IWebSocketSession session, int statusCode, string closeReason);

        /// <summary>
        /// Sends the pong.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="pong">The pong.</param>
        void SendPong(IWebSocketSession session, byte[] pong);

        /// <summary>
        /// Sends the ping.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="ping">The ping.</param>
        void SendPing(IWebSocketSession session, byte[] ping);

        /// <summary>
        /// Gets the version of current protocol.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Determines whether [is valid close code] [the specified code].
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>
        ///   <c>true</c> if [is valid close code] [the specified code]; otherwise, <c>false</c>.
        /// </returns>
        bool IsValidCloseCode(int code);
    }
}
