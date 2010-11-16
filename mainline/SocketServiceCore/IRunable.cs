using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketServiceCore.Config;
using SuperSocket.SocketServiceCore.Protocol;

namespace SuperSocket.SocketServiceCore
{
    /// <summary>
    /// Define the behavior of runable object
    /// </summary>
    public interface IRunable : ILogApp
    {
        ServiceCredentials ServerCredentials { get; set; }
        /// <summary>
        /// Setups with the specified config.
        /// </summary>
        /// <returns></returns>
        bool Setup(IServerConfig config, object protocol, string consoleBaseAddress, string assembly);

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
