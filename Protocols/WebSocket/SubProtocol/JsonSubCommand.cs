using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.WebSocket.SubProtocol
{
    /// <summary>
    /// JsonSubCommand
    /// </summary>
    /// <typeparam name="TJsonCommandInfo">The type of the json command info.</typeparam>
    public abstract class JsonSubCommand<TJsonCommandInfo> : JsonSubCommand<WebSocketSession, TJsonCommandInfo>
    {

    }

    /// <summary>
    /// JsonSubCommand
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    /// <typeparam name="TJsonCommandInfo">The type of the json command info.</typeparam>
    public abstract class JsonSubCommand<TWebSocketSession, TJsonCommandInfo> : JsonSubCommandBase<TWebSocketSession, TJsonCommandInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Gets the json message.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        protected string GetJsonMessage(TWebSocketSession session, object content)
        {
            return GetJsonMessage(session, Name, content);
        }

        /// <summary>
        /// Gets the json message.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="name">The name.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        protected string GetJsonMessage(TWebSocketSession session, string name, object content)
        {
            return GetJsonMessage(session, name, session.CurrentToken, content);
        }

        /// <summary>
        /// Sends the json message.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="content">The content.</param>
        protected void SendJsonMessage(TWebSocketSession session, object content)
        {
            session.Send(GetJsonMessage(session, content));
        }

        /// <summary>
        /// Sends the json message.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="name">The name.</param>
        /// <param name="content">The content.</param>
        protected void SendJsonMessage(TWebSocketSession session, string name, object content)
        {
            session.Send(GetJsonMessage(session, name, content));
        }
    }
}
