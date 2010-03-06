using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.Config;
using SuperSocket.Common;
using System.Threading;

namespace SuperSocket.SocketServiceCore
{
	/// <summary>
	/// Socket Session, all application session should base on this class
	/// </summary>
	public abstract class SocketSession : StreamSocketBase
	{
		public SocketSession()
		{

		}

		/// <summary>
		/// The session identity string
		/// </summary>
		private string m_SessionID = Guid.NewGuid().ToString();

		/// <summary>
		/// Gets or sets the session ID.
		/// </summary>
		/// <value>The session ID.</value>
		public string SessionID
		{
			get { return m_SessionID; }
			set { m_SessionID = value; }
		}

		private DateTime m_StartTime = DateTime.Now;

		/// <summary>
		/// Gets the session start time.
		/// </summary>
		/// <value>The session start time.</value>
		public DateTime StartTime
		{
			get { return m_StartTime; }
		}

		private DateTime m_LastActiveTime = DateTime.Now;

		/// <summary>
		/// Gets the last active time of the session.
		/// </summary>
		/// <value>The last active time.</value>
		public DateTime LastActiveTime
		{
			get { return m_LastActiveTime; }
		}

		private IServerConfig m_Config = null;

		/// <summary>
		/// Gets or sets the config.
		/// </summary>
		/// <value>The config.</value>
		public IServerConfig Config
		{
			get { return m_Config; }
			set { m_Config = value; }
		}


		public abstract void SetServer(IRunable server);
		

		/// <summary>
		/// Sets the command source.
		/// </summary>
		/// <value>The command source.</value>
		public abstract object CommandSource { set; }


		/// <summary>
		/// Starts this session.
		/// </summary>
		public abstract void Start();


		/// <summary>
		/// Executes the command.
		/// </summary>
		/// <param name="cmdInfo">The CMD info.</param>
		protected abstract void ExecuteCommand(CommandInfo cmdInfo);

		/// <summary>
		/// Says the welcome information when a client connectted.
		/// </summary>
		protected abstract void SayWelcome();

		private Thread m_CommandThread = null;

		/// <summary>
		/// Starts the the session with specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		public virtual void Start(SocketContext context)
		{
			m_CommandThread = Thread.CurrentThread;

			//Client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
			Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);			

			InitStream(context);
			
			SayWelcome();

			CommandInfo cmdInfo;

			while (TryGetCommand(out cmdInfo))
			{
				m_LastActiveTime = DateTime.Now;
                context.Status = SocketContextStatus.Healthy;

                try
                {
                    ExecuteCommand(cmdInfo);
                    context.PrevCommand = cmdInfo.Name;
                    m_LastActiveTime = DateTime.Now;

                    if (Client == null && !IsClosed)
                    {
                        //Has been closed
                        OnClose();
                        return;
                    }	
                }
                catch (SocketException)
                {
                    Close();
                    break;
                }
                catch (Exception e)
                {
                    LogUtil.LogError(e);
					HandleExceptionalError(e);
                }	
			}

            if (Client != null)
            {
                Close();
            }
            else if (!IsClosed)
            {
                OnClose();
            }
		}


		public override void Close()
		{
			if (m_CommandThread != null)
			{
				m_CommandThread.Abort();
				m_CommandThread = null;				
			}

			base.Close();
		}

		/// <summary>
		/// Called when [close].
		/// </summary>
		protected override void OnClose()
		{
			if (Closed != null)
			{
				Closed.Invoke(m_SessionID, EventArgs.Empty);
			}

            base.OnClose();
		}

		/// <summary>
		/// Occurs when [closed].
		/// </summary>
		public event EventHandler Closed;

        protected abstract void HandleExceptionalError(Exception e);
    }
}
