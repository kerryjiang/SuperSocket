using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketServiceCore.Command
{
    public interface ICommandLoader
    {
        List<ICommand<TAppSession>> LoadCommands<TAppSession>() where TAppSession : IAppSession, IAppSession<TAppSession>, new();
    }
}
