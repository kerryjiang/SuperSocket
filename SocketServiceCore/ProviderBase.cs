using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using SuperSocket.SocketServiceCore.Config;

namespace SuperSocket.SocketServiceCore
{
	/// <summary>
	/// The base class of the service provider
	/// </summary>
	public abstract class ProviderBase
	{

		/// <summary>
		/// Gets the provider name.
		/// </summary>
		/// <value>The name.</value>
		public abstract string Name { get; }

        private IServerConfig m_Config = null;

        protected IServerConfig Config
        {
            get { return m_Config; }
        }

		/// <summary>
		/// Initialize a provider from a config
		/// </summary>
		/// <param name="config">The config.</param>
		/// <returns></returns>
        public virtual bool Init(IServerConfig config)
		{
			this.m_Config = config;            
			return true;
		}

		/// <summary>
		/// Called when [service stop].
		/// </summary>
		public abstract void OnServiceStop();

        private ResourceManager m_Resource;

        public ResourceManager Resource
        {
            get { return m_Resource; }
            set { m_Resource = value; }
        }
	}
}
