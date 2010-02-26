using System;
using System.Collections.Generic;
using System.Text;
using GiantSoft.SocketServiceCore.Command;

namespace GiantSoft.SocketServiceCore
{
	/// <summary>
	/// Define the behavior of command source
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ICommandSource<T> where T : SocketSession
	{
		ICommand<T> GetCommandByName(string commandName);
	}
}
