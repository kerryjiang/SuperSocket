using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using System.Collections.Specialized;
using System.Configuration;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Root configuration model
    /// </summary>
    [Serializable]
    public partial class RootConfig : IRootConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RootConfig"/> class.
        /// </summary>
        /// <param name="rootConfig">The root config.</param>
        public RootConfig(IRootConfig rootConfig)
        {
            rootConfig.CopyPropertiesTo(this);
            this.OptionElements = rootConfig.OptionElements;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RootConfig"/> class.
        /// </summary>
        public RootConfig()
        {
            int maxWorkingThread, maxCompletionPortThreads;
            ThreadPool.GetMaxThreads(out maxWorkingThread, out maxCompletionPortThreads);
            MaxWorkingThreads = maxWorkingThread;
            MaxCompletionPortThreads = maxCompletionPortThreads;

            int minWorkingThread, minCompletionPortThreads;
            ThreadPool.GetMinThreads(out minWorkingThread, out minCompletionPortThreads);
            MinWorkingThreads = minWorkingThread;
            MinCompletionPortThreads = minCompletionPortThreads;

            PerformanceDataCollectInterval = 60;

            Isolation = IsolationMode.None;
        }

        #region IRootConfig Members

        /// <summary>
        /// Gets/Sets the max working threads.
        /// </summary>
        public int MaxWorkingThreads { get; set; }

        /// <summary>
        /// Gets/sets the min working threads.
        /// </summary>
        public int MinWorkingThreads { get; set; }

        /// <summary>
        /// Gets/sets the max completion port threads.
        /// </summary>
        public int MaxCompletionPortThreads { get; set; }

        /// <summary>
        /// Gets/sets the min completion port threads.
        /// </summary>
        public int MinCompletionPortThreads { get; set; }

        /// <summary>
        /// Gets/sets the performance data collect interval, in seconds.
        /// </summary>
        public int PerformanceDataCollectInterval { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether [disable performance data collector].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [disable performance data collector]; otherwise, <c>false</c>.
        /// </value>
        public bool DisablePerformanceDataCollector { get; set; }

        /// <summary>
        /// Gets/sets the isolation mode.
        /// </summary>
        public IsolationMode Isolation { get; set; }

        /// <summary>
        /// Gets/sets the log factory name.
        /// </summary>
        /// <value>
        /// The log factory.
        /// </value>
        public string LogFactory { get; set; }

        /// <summary>
        /// Gets/sets the option elements.
        /// </summary>
        public NameValueCollection OptionElements { get; set; }

        /// <summary>
        /// Gets the child config.
        /// </summary>
        /// <typeparam name="TConfig">The type of the config.</typeparam>
        /// <param name="childConfigName">Name of the child config.</param>
        /// <returns></returns>
        public virtual TConfig GetChildConfig<TConfig>(string childConfigName)
            where TConfig : ConfigurationElement, new()
        {
            return this.OptionElements.GetChildConfig<TConfig>(childConfigName);
        }

        #endregion
    }
}
