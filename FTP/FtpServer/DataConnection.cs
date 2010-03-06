using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore;
using SuperSocket.SocketServiceCore.Config;
using SuperSocket.FtpService.Storage;


namespace SuperSocket.FtpService
{
	class DataConnection : StreamSocketBase
	{
		private Random m_Random = new Random();
	
		private string m_Address = string.Empty;

		public string Address
		{
			get { return m_Address; }
			set { m_Address = value; }
		}
		
		private int m_Port = 0;

		public int Port
		{
			get { return m_Port; }
			set { m_Port = value; }
		}
		
		public DataConnection(FtpSession session)
		{
			m_Address			= session.Config.Ip;
			SecureProtocol		= session.Context.DataSecureProtocol;
			m_Port				= GetPassivePort(session);
			session.DataConn	= this;
		}

		public DataConnection(FtpSession session, int port) //PORT
		{
			m_Address		= session.Config.Ip;
			SecureProtocol	= session.Context.DataSecureProtocol;
			if (TrySocketPort(port))
			{
				m_Port	= port;
				session.DataConn = this;
			}
		}

		public bool RunDataConnection(FtpSession session)
		{
			TcpListener listener = null;

			try
			{
				IPAddress address	= IPAddress.Parse(m_Address);
				IPEndPoint endPoint = new IPEndPoint(address, m_Port);
				listener = new TcpListener(endPoint);
				listener.Start();
				listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
				this.Client = listener.AcceptTcpClient();
				InitStream(session.Context);
				return true;
			}
			catch (Exception e)
			{
				LogUtil.LogError("Create dataconnection failed, " + m_Address + ", " + m_Port + ":" + e.Message);
				return false;
			}
			finally
			{
				if (listener != null)
				{
					listener.Stop();
					listener = null;
				}
			}
		}

		private bool TrySocketPort(int tryPort)
		{
			TcpListener listener = null;

			try
			{
				IPAddress address	= IPAddress.Parse(m_Address);
				IPEndPoint endPoint = new IPEndPoint(address, tryPort);
				listener = new TcpListener(endPoint);
				listener.Start();
				return true;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				if (listener != null)
				{
					try
					{
						listener.Stop();
					}
					finally
					{
						listener = null;
					}
				}
			}
		}

		private int GetPassivePort(FtpSession session)
		{
			int tryPort = session.Server.FtpServiceProvider.GetRandomPort();
			int previousPort = tryPort;
			int tryTimes = 0;

			while (!TrySocketPort(tryPort))
			{
				tryTimes++;
				if (tryTimes > 5)
				{
					return 0;
				}

				tryPort = session.Server.FtpServiceProvider.GetRandomPort();
				if (previousPort == tryPort)
				{
					return 0;
				}
			}

			return tryPort;
		}

		private const string DELIM = " ";
		private const string NEWLINE = "\r\n";

		public void SendResponse(FtpContext context, List<ListItem> list)
		{
			Stream stream = GetStream(context);

			if (list == null || list.Count <= 0)
				return;

			byte[] data = new byte[0];

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < list.Count; i++)
			{
				sb.Append(list[i].Permission);

				sb.Append(DELIM);
				sb.Append(DELIM);
				sb.Append(DELIM);

				sb.Append("1");
				sb.Append(DELIM);
				sb.Append("user");
				sb.Append(DELIM);
				sb.Append(DELIM);
				sb.Append(DELIM);
				sb.Append(DELIM);
				sb.Append(DELIM);
				sb.Append("group");

				sb.Append(DELIM);

				sb.Append(GetFixedLength(list[i].Length));

				sb.Append(DELIM);

				sb.Append(GetListTimeString(list[i].LastModifiedTime));

				sb.Append(DELIM);

				sb.Append(list[i].Name);

				sb.Append(NEWLINE);

				data = context.Charset.GetBytes(sb.ToString());
				stream.Write(data, 0, data.Length);

				sb.Remove(0, sb.Length);
			}

			stream.Flush();
		}

		private string GetListTimeString(DateTime time)
		{
			if (time.Year == DateTime.Now.Year)
				return time.ToString("MMM dd hh:mm");
			else
				return time.ToString("MMM dd yyyy");
		}

		private string GetFixedLength(long length)
		{
			string size = length.ToString();

			size = size.PadLeft(11, ' ');

			return size;
		}

		protected override void OnClose()
		{
			//Do nothing
		}
	}
}
