using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.Config;


namespace SuperSocket.SocketServiceCore
{
	/// <summary>
	/// The base class which provide the communication function with client
	/// </summary>
	public abstract class StreamSocketBase
	{
		protected Stream m_Stream = null;

		private Socket m_Client = null;

		/// <summary>
		/// Gets or sets the client.
		/// </summary>
		/// <value>The client.</value>
        public Socket Client
		{
			get { return m_Client; }
			set
			{
				m_Client = value;								
				m_LocalEndPoint = (IPEndPoint)m_Client.LocalEndPoint;
				m_RemoteEndPoint = (IPEndPoint)m_Client.RemoteEndPoint;
			}
		}

        private bool m_IsClosed = false;

        protected bool IsClosed
        {
            get { return m_IsClosed; }
        }
		
		private IPEndPoint m_LocalEndPoint = null;

		/// <summary>
		/// Gets the local end point.
		/// </summary>
		/// <value>The local end point.</value>
		public IPEndPoint LocalEndPoint
		{
			get { return m_LocalEndPoint; }
		}

		private IPEndPoint m_RemoteEndPoint = null;

		/// <summary>
		/// Gets the remote end point.
		/// </summary>
		/// <value>The remote end point.</value>
		public IPEndPoint RemoteEndPoint
		{
			get { return m_RemoteEndPoint; }
		}

		private SslProtocols m_SecureProtocol = SslProtocols.None;

		/// <summary>
		/// Gets or sets the secure protocol.
		/// </summary>
		/// <value>The secure protocol.</value>
		public SslProtocols SecureProtocol
		{
			get { return m_SecureProtocol; }
			set { m_SecureProtocol = value; }
		}		

		private StreamReader m_Reader = null;
		
		protected StreamReader SocketReader
		{
			get { return m_Reader; }
		}

		/// <summary>
		/// Gets the stream.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public Stream GetStream(SocketContext context)
		{
			if (m_Stream == null)
			{
				InitStream(context);
			}

			return m_Stream;
		}

		/// <summary>
		/// Initialize the stream.
		/// </summary>
		/// <param name="context">The context.</param>
		public void InitStream(SocketContext context)
		{
			switch (m_SecureProtocol)
			{
				case (SslProtocols.Tls):
				case (SslProtocols.Ssl3):
				case (SslProtocols.Ssl2):
					SslStream sslStream = new SslStream(new NetworkStream(Client), false);
					sslStream.AuthenticateAsServer(AuthenticationManager.GetCertificate(), false, SslProtocols.Default, true);
					m_Stream = sslStream as Stream;
					break;
				default:
                    m_Stream = new NetworkStream(Client);
					break;
			}
			
			m_Stream.ReadTimeout	= 300000; // 5 mins
			m_Stream.WriteTimeout	= 300000; // 5 mins

			if (context == null)
				m_Reader = new StreamReader(m_Stream, Encoding.Default);
			else
				m_Reader = new StreamReader(m_Stream, context.Charset);
		}

		/// <summary>
		/// Tries to get command from clinet.
		/// </summary>
		/// <param name="cmdInfo">The CMD info.</param>
		/// <returns></returns>
        protected virtual bool TryGetCommand(out string command)
		{
			command = string.Empty;
			
			try
			{			
				command = m_Reader.ReadLine();
			}
			catch(Exception e)
			{
				LogUtil.LogError(e);
                this.Close();
				return false;
			}
			
			if (string.IsNullOrEmpty(command))
				return false;

			command = command.Trim();		

			if (string.IsNullOrEmpty(command))
				return false;

            return true;
		}

		/// <summary>
		/// Closes this connection.
		/// </summary>
		public virtual void Close()
		{
            if (m_Client != null && !m_IsClosed)
			{
                try
                {
                    m_Client.Shutdown(SocketShutdown.Both);                                        
                }
                catch (Exception e)
                {
                    LogUtil.LogError(e);
                }

                try
                {
                    m_Client.Close();
                }
                catch (Exception e)
                {
                    LogUtil.LogError(e);
                }
                finally
                {
                    m_Client = null;
                    m_IsClosed = true;
                    OnClose();
                }				
			}
		}

		/// <summary>
		/// Called when [close].
		/// </summary>
        protected virtual void OnClose()
        {
            m_IsClosed = true;
        }

		/// <summary>
		/// Sends the response to client.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="message">The message should be send to clinet.</param>
		public virtual void SendResponse(SocketContext context, string message)
		{
			if(string.IsNullOrEmpty(message))
				return;

            if (!message.EndsWith(Environment.NewLine))
                message = message + Environment.NewLine;
				
			byte[] data	= context.Charset.GetBytes(message);

            try
            {
                m_Stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                LogUtil.LogError(e);
                this.Close();
            }
		}
	}
}
