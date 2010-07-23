using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Text;
using SuperSocket.SocketServiceCore.Config;
using SuperSocket.Common;

namespace SuperSocket.SocketServiceCore
{
    /// <summary>
    /// Define the behavior of runable object
    /// </summary>
    public interface IRunable : ILogApp
    {
        ServiceCredentials ServerCredentials { get; set; }
        /// <summary>
        /// Setups the specified config.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        bool Setup(string assembly, IServerConfig config, string consoleBaseAddress);

        /// <summary>
        /// Starts with the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        bool Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();

    }

    public interface IRunable<T> : IRunable where T : IAppSession
    {

    }
}
