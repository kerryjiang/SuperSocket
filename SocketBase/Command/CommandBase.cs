using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase.Command
{
    /// <summary>
    /// Command base class
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TPackageInfo">The type of the request info.</typeparam>
    public abstract class CommandBase<TAppSession, TPackageInfo> : ICommand<TAppSession, TPackageInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TPackageInfo>, new()
        where TPackageInfo : IPackageInfo
    {

        #region ICommand<TAppSession,TPackageInfo> Members

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="packageInfo">The package info.</param>
        public abstract void ExecuteCommand(TAppSession session, TPackageInfo packageInfo);

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
