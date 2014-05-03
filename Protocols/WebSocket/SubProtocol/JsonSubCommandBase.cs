using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.WebSocket.SubProtocol
{
    /// <summary>
    /// Json SubCommand base
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    /// <typeparam name="TJsonCommandInfo">The type of the json command info.</typeparam>
    public abstract class JsonSubCommandBase<TWebSocketSession, TJsonCommandInfo> : SubCommandBase<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        private const string m_QueryTemplateA = "{0}-{1} {2}";
        private const string m_QueryTemplateB = "{0} {1}";

        private bool m_IsSimpleType = false;

        private Type m_CommandInfoType;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSubCommandBase&lt;TWebSocketSession, TJsonCommandInfo&gt;"/> class.
        /// </summary>
        public JsonSubCommandBase()
        {
            m_CommandInfoType = typeof(TJsonCommandInfo);

            if (m_CommandInfoType.IsSimpleType())
                m_IsSimpleType = true;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        public override void ExecuteCommand(TWebSocketSession session, SubRequestInfo requestInfo)
        {
            if (string.IsNullOrEmpty(requestInfo.Body))
            {
                ExecuteJsonCommand(session, default(TJsonCommandInfo));
                return;
            }

            TJsonCommandInfo jsonCommandInfo;

            LocalDataStoreSlot tokenSlot = null;

            if (!string.IsNullOrEmpty(requestInfo.Token))
                tokenSlot = session.SetCurrentToken(requestInfo.Token);

            try
            {

                if (!m_IsSimpleType)
                    jsonCommandInfo = (TJsonCommandInfo)session.AppServer.JsonDeserialize(requestInfo.Body, m_CommandInfoType);
                else
                    jsonCommandInfo = (TJsonCommandInfo)Convert.ChangeType(requestInfo.Body, m_CommandInfoType);

                ExecuteJsonCommand(session, jsonCommandInfo);
            }
            finally
            {
                if (tokenSlot != null)
                    Thread.SetData(tokenSlot, null);
            }
        }

        /// <summary>
        /// Executes the json command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="commandInfo">The command info.</param>
        protected abstract void ExecuteJsonCommand(TWebSocketSession session, TJsonCommandInfo commandInfo);

        /// <summary>
        /// Gets the json message.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="name">The name.</param>
        /// <param name="token">The token.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        protected string GetJsonMessage(TWebSocketSession session, string name, string token, object content)
        {
            string strOutput;

            //Needn't serialize primitive type object
            if (content.GetType().IsSimpleType())
                strOutput = content.ToString();
            else
                strOutput = session.AppServer.JsonSerialize(content);

            if (string.IsNullOrEmpty(token))
                return string.Format(m_QueryTemplateB, name, strOutput);
            else
                return string.Format(m_QueryTemplateA, name, token, strOutput);
        }
    }
}
