using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Command
{
    public abstract class CommandBase<TAppSession, TCommandInfo> : ICommand<TAppSession, TCommandInfo>
        where TAppSession : IAppSession, IAppSession<TAppSession, TCommandInfo>, new()
        where TCommandInfo : ICommandInfo
    {

        #region ICommand<TAppSession,TCommandInfo> Members

        public abstract void ExecuteCommand(TAppSession session, TCommandInfo commandInfo);

        #endregion

        #region ICommand Members

        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        #endregion
    }
}
