using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketServiceCore.Command
{
    public interface ICommand<T> where T : IAppSession
	{
		void ExecuteCommand(T session, CommandInfo commandData);
	}
}
