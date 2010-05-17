using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.Common;

namespace SuperSocket.SocketServiceCore
{
	class CommandReceiveState
	{
		private const int m_CmdBufferSize = 1024;

		private byte[] m_CmdBuffer = new byte[m_CmdBufferSize];

		public byte[] CmdBuffer
		{
			get { return m_CmdBuffer; }
		}

		private List<byte> m_CmdBuilder = new List<byte>();

		public List<byte> CmdBuilder
		{
			get { return m_CmdBuilder; }
		}

		public SocketContext Context { get; set; }

		public override string ToString()
		{
			return Encoding.ASCII.GetString(m_CmdBuilder.ToArray());
		}
	}

    public class CommandExecutionState
    {
        public SocketContext Context { get; set; }
        public EventHandler<CommandInfo> ExecuteCommandDelegate { get; set; }
        public CommandInfo CurrentCommand { get; set; }
    }

	class ResponseSendState
	{
		public SocketContext Context { get; set; }
	}

    public delegate void EventHandler<T>(T param);

    public class AsyncSocketSession<T> : SocketSession<T>
        where T : IAppSession, new()
	{
		private CommandReceiveState m_ReceiveState = new CommandReceiveState();
		private ResponseSendState m_SendState = new ResponseSendState();

        public AsyncSocketSession()
        {

        }

		protected override void Start(SocketContext context)
		{
			Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);	
			m_ReceiveState.Context = context;
			InitStream(context);
			SayWelcome();
			this.GetStream(context).BeginRead(m_ReceiveState.CmdBuffer, 0, m_ReceiveState.CmdBuffer.Length, new AsyncCallback(OnEndReceive), m_ReceiveState);
		}

		private void OnEndReceive(IAsyncResult result)
		{
			CommandReceiveState receiveState = result.AsyncState as CommandReceiveState;
            int read = 0;

            SocketError socketError;

            try
            {
                read = this.Client.Client.EndReceive(result, out socketError);
            }
            catch (Exception e)
            {
                LogUtil.LogError(e);
                socketError = SocketError.Fault;
            }

            if (socketError != SocketError.Success)
            {
                Close();
                return;
            }

			string thisReadContent = Encoding.ASCII.GetString(receiveState.CmdBuffer, 0, read);

			//m_LastActiveTime = DateTime.Now;
			if (!string.IsNullOrEmpty(thisReadContent) && thisReadContent.EndsWith("\r\n"))
			{
				receiveState.CmdBuilder.AddRange(receiveState.CmdBuffer.Take(read));
				string command = receiveState.ToString();
				receiveState.CmdBuilder.Clear();

				if (string.IsNullOrEmpty(command))
					return;

				command = command.Trim();

				if (string.IsNullOrEmpty(command))
					return;

				CommandInfo cmdInfo = new CommandInfo(command);

                var executeDelegate = new EventHandler<CommandInfo>(ExecuteCommand);
                executeDelegate.BeginInvoke(cmdInfo, new AsyncCallback(OnEndExecuteCommand),
                    new CommandExecutionState
                    {
                        Context = receiveState.Context,
                        ExecuteCommandDelegate = executeDelegate,
                        CurrentCommand = cmdInfo
                    });
			}
			else
			{
                this.Client.Client.BeginReceive(receiveState.CmdBuffer, 0, receiveState.CmdBuffer.Length, System.Net.Sockets.SocketFlags.Broadcast, new AsyncCallback(OnEndReceive), receiveState);
			}
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

            this.Client.Client.BeginReceive(m_ReceiveState.CmdBuffer,
                    0, m_ReceiveState.CmdBuffer.Length,
                    System.Net.Sockets.SocketFlags.None,
                    new AsyncCallback(OnEndReceive), m_ReceiveState);            
        }

		public override void SendResponse(SocketContext context, string message)
		{
			if (string.IsNullOrEmpty(message))
				return;

			if (!message.EndsWith(Environment.NewLine))
                message = message + Environment.NewLine;

			byte[] data = context.Charset.GetBytes(message);

			m_SendState.Context = context;

			GetStream(context).BeginWrite(data, 0, data.Length, new AsyncCallback(OnEndSend), m_SendState);
		}

		private void OnEndSend(IAsyncResult result)
		{
			SocketContext context = result as SocketContext;
			GetStream(context).EndWrite(result);
			this.GetStream(context).BeginRead(m_ReceiveState.CmdBuffer, 0, m_ReceiveState.CmdBuffer.Length, new AsyncCallback(OnEndReceive), m_ReceiveState);
		}
	}
}
