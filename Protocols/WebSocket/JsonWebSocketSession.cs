using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Json websocket session
    /// </summary>
    public class JsonWebSocketSession : JsonWebSocketSession<JsonWebSocketSession>
    {

    }

    /// <summary>
    /// Json websocket session
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    public class JsonWebSocketSession<TWebSocketSession> : WebSocketSession<TWebSocketSession>
        where TWebSocketSession : JsonWebSocketSession<TWebSocketSession>, new()
    {
        private const string m_QueryTemplate = "{0} {1}";

        private string GetJsonMessage(string name, object content)
        {
            if(content.GetType().IsSimpleType())
                return string.Format(m_QueryTemplate, name, content);
            else
                return string.Format(m_QueryTemplate, name, AppServer.JsonSerialize(content));
        }

        /// <summary>
        /// Sends the json message.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="content">The content.</param>
        public void SendJsonMessage(string name, object content)
        {
            this.Send(GetJsonMessage(name, content));
        }
    }
}
