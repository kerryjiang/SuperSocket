using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.WebSocket.SubProtocol
{
    /// <summary>
    /// SubCommand base
    /// </summary>
    public abstract class SubCommandBase : SubCommandBase<WebSocketSession>
    {

    }

    /// <summary>
    /// SubCommand base
    /// </summary>
    /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
    public abstract class SubCommandBase<TWebSocketSession> : ISubCommand<TWebSocketSession>, ISubCommandFilterLoader
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        #region ISubCommand Members

        /// <summary>
        /// Gets the name.
        /// </summary>
        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        void ISubCommand<TWebSocketSession>.ExecuteCommand(TWebSocketSession session, SubRequestInfo requestInfo)
        {
            var filters = m_Filters;

            if (filters == null || filters.Length == 0)
            {
                ExecuteCommand(session, requestInfo);
                return;
            }

            var commandContext = new CommandExecutingContext();
            commandContext.Initialize(session, requestInfo, this);

            for (var i = 0; i < filters.Length; i++)
            {
                var filter = filters[i];

                filter.OnCommandExecuting(commandContext);

                if (commandContext.Cancel)
                    break;
            }

            if (!commandContext.Cancel)
            {
                ExecuteCommand(session, requestInfo);

                for (var i = 0; i < filters.Length; i++)
                {
                    var filter = filters[i];
                    filter.OnCommandExecuted(commandContext);
                }
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        public abstract void ExecuteCommand(TWebSocketSession session, SubRequestInfo requestInfo);

        #endregion

        private SubCommandFilterAttribute[] m_Filters;

        void ISubCommandFilterLoader.LoadSubCommandFilters(IEnumerable<SubCommandFilterAttribute> globalFilters)
        {
            var filters = new List<SubCommandFilterAttribute>();

            if (globalFilters.Any())
            {
                filters.AddRange(globalFilters);
            }

            var commandFilters = this.GetType().GetCustomAttributes(true).OfType<SubCommandFilterAttribute>().ToArray();

            if (commandFilters.Any())
            {
                filters.AddRange(commandFilters);
            }

            m_Filters = filters.OrderBy(f => f.Order).ToArray();
        }
    }
}
