using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.WebSocket.SubProtocol
{
    /// <summary>
    /// Async json sub command
    /// </summary>
    /// <typeparam name="TJsonCommandInfo">The type of the json command info.</typeparam>
    public abstract class AsyncJsonSubCommand<TJsonCommandInfo> : AsyncJsonSubCommand<WebSocketSession, TJsonCommandInfo>
    {

    }

    /// <summary>
    /// Async json sub command
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    /// <typeparam name="TJsonCommandInfo">The type of the json command info.</typeparam>
    public abstract class AsyncJsonSubCommand<TWebSocketSession, TJsonCommandInfo> : JsonSubCommandBase<TWebSocketSession, TJsonCommandInfo>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        private Action<TWebSocketSession, string, TJsonCommandInfo> m_AsyncJsonCommandAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncJsonSubCommand&lt;TWebSocketSession, TJsonCommandInfo&gt;"/> class.
        /// </summary>
        public AsyncJsonSubCommand()
        {
            m_AsyncJsonCommandAction = ExecuteAsyncJsonCommand;
        }

        /// <summary>
        /// Executes the json command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="commandInfo">The command info.</param>
        protected override void ExecuteJsonCommand(TWebSocketSession session, TJsonCommandInfo commandInfo)
        {
            m_AsyncJsonCommandAction.BeginInvoke(session, session.CurrentToken, commandInfo, null, session);
        }

        /// <summary>
        /// Executes the async json command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="token">The token.</param>
        /// <param name="commandInfo">The command info.</param>
        protected abstract void ExecuteAsyncJsonCommand(TWebSocketSession session, string token, TJsonCommandInfo commandInfo);

        private void OnAsyncJsonCommandExecuted(IAsyncResult result)
        {
            var session = (TWebSocketSession)result.AsyncState;

            try
            {
                m_AsyncJsonCommandAction.EndInvoke(result);
            }
            catch (Exception e)
            {
                session.Logger.Error(e);
            }
        }

        /// <summary>
        /// Sends the json message.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="token">The token.</param>
        /// <param name="content">The content.</param>
        protected void SendJsonMessage(TWebSocketSession session, string token, object content)
        {
            session.Send(GetJsonMessage(session, this.Name, token, content));
        }

        /// <summary>
        /// Sends the json message.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="name">The name.</param>
        /// <param name="token">The token.</param>
        /// <param name="content">The content.</param>
        protected void SendJsonMessage(TWebSocketSession session, string name, string token, object content)
        {
            session.Send(GetJsonMessage(session, name, token, content));
        }
    }
}
