using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Reflection;
using SuperSocket.SocketServiceCore.Command;
using SuperSocket.SocketServiceCore.Config;
using System.ServiceModel.Description;
using SuperSocket.Common;
using System.IO;
using System.Collections;
using System.Net.Sockets;
using System.Threading;

namespace SuperSocket.SocketServiceCore
{
	public abstract class SocketServerBase<T> : IRunable, ICommandSource<T> where T : SocketSession, new()
	{
		protected object SyncRoot = new object();

		private IPEndPoint m_EndPoint;

		public IPEndPoint EndPoint
		{
			get { return m_EndPoint; }
			set { m_EndPoint = value; }
		}
				
		private Dictionary<string, T> m_SessionDict = new Dictionary<string, T>();

		protected Dictionary<string, T> SessionDict
		{
			get
			{
				return m_SessionDict;
			}
		}

		public int SessionCount
		{
			get{ return m_SessionDict.Count; }
		}

		private Dictionary<string, ICommand<T>> dictCommand = new Dictionary<string, ICommand<T>>();
		
		public SocketServerBase()
		{
			LoadCommands();
		}

		public SocketServerBase(IPEndPoint localEndPoint)
		{
			m_EndPoint = localEndPoint;
			LoadCommands();
		}

		/// <summary>
		/// Loads the command dictionary.
		/// </summary>
		private void LoadCommands()
		{			
			Type commandType	= typeof(ICommand<T>);
			Assembly asm		= typeof(T).Assembly;
			Type[] arrType		= asm.GetTypes();
			
			for (int i = 0; i < arrType.Length; i++)
			{
				//LogUtil.LogInfo(arrType[i].ToString() + "\r\n");
				Type[] arrInterface = arrType[i].GetInterfaces();
				
				for(int j = 0; arrInterface!=null && j < arrInterface.Length; j++)
				{
					if (arrInterface[j] == commandType)
					{
						//LogUtil.LogInfo(arrInterface[j].ToString() + ":" + commandType.ToString());
						dictCommand[arrType[i].Name.ToUpper()] = arrType[i].GetConstructor(new Type[0]).Invoke(new object[0]) as ICommand<T>;
					}
				}				
			}
			//LogUtil.LogInfo("Load " + dictCommand.Count + " commands from " + arrType.Length + " types!");		
		}

		#region ICommandSource<T> Members

		public ICommand<T> GetCommandByName(string commandName)
		{
			throw new NotImplementedException();
		}

		#endregion

		private IServerConfig m_Config = null;

		protected IServerConfig Config
		{
			get { return m_Config; }
		}

		private ServiceCredentials m_ServerCredentials;

		public ServiceCredentials ServerCredentials
		{
			get { return m_ServerCredentials; }
			set { m_ServerCredentials = value; }
		}

		public virtual bool Start()
		{
			if (m_Config.EnableManagementService)
			{
				if (!StartManagementService())
				{
					LogUtil.LogError("Failed to run manage service in " + this.GetType().ToString());
					return false;
				}
			}

			return true;
		}

		protected virtual T RegisterSession(TcpClient client)
		{
			T session = new T();
			session.SetServer(this);
			session.CommandSource = this;
			session.Client = client;
			session.Config = Config;
			session.Closed += new EventHandler(session_Closed);
			lock (SyncRoot)
			{
				m_SessionDict[session.SessionID] = session;
			}
			LogUtil.LogInfo("SocketSession " + session.SessionID + " was accepted!");
			return session;
		}

		/// <summary>
		/// Handles the Closed event of the session control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void session_Closed(object sender, EventArgs e)
		{
			//the sender is a sessionID
			string sessionID = sender as string;

			if (!string.IsNullOrEmpty(sessionID))
			{
				try
				{
					SessionDict.Remove(sessionID);
					LogUtil.LogInfo("SocketSession " + sessionID + " was closed!");
				}
				catch (Exception exc)
				{
					LogUtil.LogError(exc);
				}
			}
		}

		public virtual void Stop()
		{
			if (m_ClearIdleSessionTimer != null)
			{
				m_ClearIdleSessionTimer.Enabled = false;
				m_ClearIdleSessionTimer.Dispose();
				m_ClearIdleSessionTimer = null;
			}
		}


		private Dictionary<string, ProviderBase> m_ProviderDict = new Dictionary<string, ProviderBase>();

		/// <summary>
		/// Setups the specified factory.
		/// </summary>
		/// <param name="assembly">The assembly name.</param>
		/// <param name="config">The config.</param>
		/// <returns></returns>
		public virtual bool Setup(string assembly, IServerConfig config)
		{
			m_Config = config;

			if (string.IsNullOrEmpty(assembly))
				return true;

			string dir = FileHelper.GetParentFolder(this.GetType().Assembly.Location);

			string assemblyFile = Path.Combine(dir, assembly + ".dll");

			try
			{
				Type typeProvider = typeof(ProviderBase);

				Assembly ass = Assembly.LoadFrom(assemblyFile);

				Type[] arrType = ass.GetTypes();

				for (int i = 0; arrType != null && i < arrType.Length; i++)
				{
					//Must be a seal class
					if (arrType[i].IsSubclassOf(typeProvider))
					{
						ProviderBase provider = ass.CreateInstance(arrType[i].ToString()) as ProviderBase;
						if (provider.Init(config))
						{
							m_ProviderDict[provider.Name.ToLower()] = provider;
						}
						else
						{
							LogUtil.LogError("Failed to initalize provider " + arrType[i].ToString() + "!");
							return false;
						}
					}
				}

				if (!Ready)
				{
					LogUtil.LogError("Failed to load service provider from assembly:" + assemblyFile);
					return false;
				}

				return true;
			}
			catch (Exception e)
			{
				LogUtil.LogError(e);
				return false;
			}
		}

		/// <summary>
		/// Gets service provider by name.
		/// </summary>
		/// <param name="providerName">Name of the provider.</param>
		/// <returns></returns>
		protected ProviderBase GetProviderByName(string providerName)
		{
			ProviderBase provider = null;

			if (m_ProviderDict.TryGetValue(providerName.ToLower(), out provider))
			{
				return provider;
			}
			else
			{
				return null;
			}
		}

		protected abstract bool Ready { get; }

		protected abstract bool StartManagementService();

		protected void SetupClearSessionTimer()
		{
			m_ClearIdleSessionTimer = new System.Timers.Timer();
			m_ClearIdleSessionTimer.Interval = 60 * 1000;
			m_ClearIdleSessionTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_ClearIdleSessionTimer_Elapsed);
			m_ClearIdleSessionTimer.Enabled = true;
		}

		void m_ClearIdleSessionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			ClearIdleSession();
		}

		private System.Timers.Timer m_ClearIdleSessionTimer = null;

		private void ClearIdleSession()
		{
			List<SocketSession> idleSessions = new List<SocketSession>();

			IEnumerator enu = SessionDict.Values.GetEnumerator();

			while (enu.MoveNext())
			{
				SocketSession session = enu.Current as SocketSession;

				if (DateTime.Now.Subtract(session.LastActiveTime).TotalMinutes > 5)
				{
					idleSessions.Add(session);
				}
			}

			foreach (SocketSession session in idleSessions)
			{
				session.Close();
			}
		}

        protected void ExecuteAsync(string stepName, WaitCallback callback)
        {
            this.ExecuteAsync(stepName, callback, null);
        }

        protected void ExecuteAsync(string stepName, WaitCallback callback, ExceptionCallback exceptionCallback)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    callback(null);
                }
                catch (Exception e)
                {
                    LogUtil.LogError(stepName, e);
                    if (exceptionCallback != null)
                    {
                        try
                        {
                            exceptionCallback();
                        }
                        catch (Exception exc)
                        {
                            LogUtil.LogError(stepName + " exception callback", exc);
                        }
                    }
                }
            });
        }

        protected delegate void ExceptionCallback();
	}
}
