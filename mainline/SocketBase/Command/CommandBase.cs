using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase.Command
{
    public abstract class CommandBase<TAppSession, TRequestInfo> : ICommand<TAppSession, TRequestInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TRequestInfo>, new()
        where TRequestInfo : IRequestInfo
    {

        #region ICommand<TAppSession,TRequestInfo> Members

        public abstract void ExecuteCommand(TAppSession session, TRequestInfo requestInfo);

        #endregion

        #region ICommand Members

        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        #endregion
    }
}
