using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketServiceCore.Command;

namespace SuperSocket.SocketServiceCore
{
    /// <summary>
    /// Define the behavior of command source
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICommandSource<T> where T : IAppSession
    {
        ICommand<T> GetCommandByName(string commandName);
    }
}
