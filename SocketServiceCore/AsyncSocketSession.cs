using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.Common;
using System.IO;

namespace SuperSocket.SocketServiceCore
{
    public class CommandExecutionState
    {
        public SocketContext Context { get; set; }
        public CommandDelegate<CommandInfo> ExecuteCommandDelegate { get; set; }
        public CommandInfo CurrentCommand { get; set; }
    }

    class ReadLineState
    {
        public ReadLineDelegate Delegate { get; set; }
        public StreamReader SocketReader { get; set; }
        public SocketContext Context { get; set; }
    }

    public delegate void CommandDelegate<T>(T e);

    public delegate string ReadLineDelegate();

    public class AsyncSocketSession<T> : SocketSession<T>
        where T : IAppSession, new()
	{
		protected override void Start(SocketContext context)
		{
			Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);	
			InitStream(context);
			SayWelcome();

            ReadLineDelegate readLineDelegate = new ReadLineDelegate(this.SocketReader.ReadLine);
            readLineDelegate.BeginInvoke(new AsyncCallback(OnEndReadLine),
                new ReadLineState
                {
                    Delegate = readLineDelegate,
                    SocketReader = this.SocketReader,
                    Context = context
                });
		}

        private void OnEndReadLine(IAsyncResult result)
        {
            ReadLineState state = result.AsyncState as ReadLineState;

            if (state == null)
                return;

            string commandLine = string.Empty;

            try
            {
                commandLine = state.Delegate.EndInvoke(result);
            }
            catch (Exception)
            {
                Close();
                return;
            }

            if (!string.IsNullOrEmpty(commandLine))
                commandLine = commandLine.Trim();

            if (string.IsNullOrEmpty(commandLine))
            {
                BeginReadCommandLine(state.Context);
                return;
            }

            CommandInfo cmdInfo = new CommandInfo(commandLine);

            var executeDelegate = new CommandDelegate<CommandInfo>(ExecuteCommand);
            executeDelegate.BeginInvoke(cmdInfo, new AsyncCallback(OnEndExecuteCommand),
                new CommandExecutionState
                {
                    Context = state.Context,
                    ExecuteCommandDelegate = executeDelegate,
                    CurrentCommand = cmdInfo
                });
        }

        private void BeginReadCommandLine(SocketContext context)
        {
            ReadLineDelegate readLineDelegate = new ReadLineDelegate(this.SocketReader.ReadLine);
            readLineDelegate.BeginInvoke(new AsyncCallback(OnEndReadLine),
                new ReadLineState
                {
                    Delegate = readLineDelegate,
                    SocketReader = this.SocketReader,
                    Context = context
                });
        }

        private void OnEndExecuteCommand(IAsyncResult result)
        {
            CommandExecutionState executionState = result.AsyncState as CommandExecutionState;

            try
            {
                executionState.ExecuteCommandDelegate.EndInvoke(result);
                executionState.Context.PrevCommand = executionState.CurrentCommand.Name;
            }
            catch (SocketException)
            {
                Close();
                return;
            }
            catch (Exception e)
            {
                LogUtil.LogError(e);
                HandleExceptionalError(e);
            }

            BeginReadCommandLine(executionState.Context);
        }
	}
}
