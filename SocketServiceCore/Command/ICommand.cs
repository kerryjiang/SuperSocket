using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.SocketServiceCore.Command
{
	public interface ICommand<T> where T : SocketSession
	{
		void Execute(T session, CommandInfo commandData);
	}
}
