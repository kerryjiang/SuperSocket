using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.SocketBase.Command
{
    /// <summary>
    /// Command basic interface
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    /// Command basic interface
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface ICommand<TAppSession, TRequestInfo> : ICommand
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        void ExecuteCommand(TAppSession session, TRequestInfo requestInfo);
    }

    /// <summary>
    /// Mockup command
    /// </summary>
    /// <typeparam name="TAppSession">The type of the app session.</typeparam>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public class MockupCommand<TAppSession, TRequestInfo> : ICommand<TAppSession, TRequestInfo>
        where TRequestInfo : IRequestInfo
        where TAppSession : IAppSession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MockupCommand&lt;TAppSession, TRequestInfo&gt;"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public MockupCommand(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="requestInfo">The request info.</param>
        public void ExecuteCommand(TAppSession session, TRequestInfo requestInfo)
        {

        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }
    }
}