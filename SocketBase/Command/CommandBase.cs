using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase.Command
{
    /// <summary>
    /// Command base class
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public abstract class CommandBase<TAppSession, TRequestInfo> : ICommand<TAppSession, TRequestInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
        where TRequestInfo : IRequestInfo
    {

        #region ICommand<TAppSession,TRequestInfo> Members

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        public abstract void ExecuteCommand(TAppSession session, TRequestInfo requestInfo);

        #endregion

        #region ICommand Members

        /// <summary>
        /// Gets the name.
        /// </summary>
        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        #endregion

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.GetType().AssemblyQualifiedName;
        }
    }
}
