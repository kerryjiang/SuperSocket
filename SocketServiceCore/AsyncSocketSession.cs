using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using SuperSocket.SocketServiceCore.Command;

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

	class ResponseSendState
	{
		public SocketContext Context { get; set; }
	}

    public class AsyncSocketSession<T> : SocketSession<T>
        where T : IAppSession, new()
	{
		private CommandReceiveState m_ReceiveState = new CommandReceiveState();
		private ResponseSendState m_SendState = new ResponseSendState();

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
			int read = this.GetStream(receiveState.Context).EndRead(result);
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

				ExecuteCommand(cmdInfo);

				receiveState.Context.PrevCommand = cmdInfo.Name;
			}
			else
			{
				this.Client.Client.BeginReceive(m_ReceiveState.CmdBuffer, 0, m_ReceiveState.CmdBuffer.Length, System.Net.Sockets.SocketFlags.None, new AsyncCallback(OnEndReceive), m_ReceiveState);
			}
		}

		public override void SendResponse(SocketContext context, string message)
		{
			if (string.IsNullOrEmpty(message))
				return;

			if (!message.EndsWith("\r\n"))
				message = message + "\r\n";

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
