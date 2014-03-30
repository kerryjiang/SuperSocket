using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Logging;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.WebSocket.Config;
using SuperSocket.SocketBase;

namespace SuperSocket.WebSocket.SubProtocol
{
    /// <summary>
    /// Default basic sub protocol implementation
    /// </summary>
    public class BasicSubProtocol : BasicSubProtocol<WebSocketSession>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        public BasicSubProtocol()
            : base(Assembly.GetCallingAssembly())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        /// <param name="name">The sub protocol name.</param>
        public BasicSubProtocol(string name)
            : base(name, Assembly.GetCallingAssembly())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        /// <param name="commandAssembly">The command assembly.</param>
        public BasicSubProtocol(Assembly commandAssembly)
            : base(commandAssembly)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        /// <param name="commandAssemblies">The command assemblies.</param>
        public BasicSubProtocol(IEnumerable<Assembly> commandAssemblies)
            : base(commandAssemblies)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        /// <param name="name">The sub protocol name.</param>
        /// <param name="commandAssembly">The command assembly.</param>
        public BasicSubProtocol(string name, Assembly commandAssembly)
            : base(name, commandAssembly)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        /// <param name="name">The sub protocol name.</param>
        /// <param name="commandAssemblies">The command assemblies.</param>
        public BasicSubProtocol(string name, IEnumerable<Assembly> commandAssemblies)
            : base(name, commandAssemblies)
        {

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="commandAssemblies">The command assemblies.</param>
        /// <param name="requestInfoParser">The request info parser.</param>
        public BasicSubProtocol(string name, IEnumerable<Assembly> commandAssemblies, IRequestInfoParser<SubRequestInfo> requestInfoParser)
            : base(name, commandAssemblies, requestInfoParser)
        {

        }
    }

    /// <summary>
    /// Default basic sub protocol implementation
    /// </summary>
    public class BasicSubProtocol<TWebSocketSession> : SubProtocolBase<TWebSocketSession>
        where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
    {
        /// <summary>
        /// Default basic sub protocol name
        /// </summary>
        public const string DefaultName = "Basic";

        private List<Assembly> m_CommandAssemblies = new List<Assembly>();

        private Dictionary<string, ISubCommand<TWebSocketSession>> m_CommandDict;

        private ILog m_Logger;

        private SubCommandFilterAttribute[] m_GlobalFilters;

        internal static BasicSubProtocol<TWebSocketSession> CreateDefaultSubProtocol()
        {
            var commandAssembly = typeof(TWebSocketSession).Assembly;

            if (commandAssembly == Assembly.GetExecutingAssembly())
                 commandAssembly = Assembly.GetEntryAssembly();

            return new BasicSubProtocol<TWebSocketSession>(DefaultName, commandAssembly);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class with the calling aseembly as command assembly
        /// </summary>
        public BasicSubProtocol()
            : this(DefaultName, Assembly.GetCallingAssembly())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class with the calling aseembly as command assembly
        /// </summary>
        /// <param name="name">The sub protocol name.</param>
        public BasicSubProtocol(string name)
            : this(name, Assembly.GetCallingAssembly())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class with command assemblies
        /// </summary>
        /// <param name="commandAssemblies">The command assemblies.</param>
        public BasicSubProtocol(IEnumerable<Assembly> commandAssemblies)
            : this(DefaultName, commandAssemblies, new BasicSubCommandParser())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class with single command assembly.
        /// </summary>
        /// <param name="commandAssembly">The command assembly.</param>
        public BasicSubProtocol(Assembly commandAssembly)
            : this(DefaultName, new List<Assembly> { commandAssembly }, new BasicSubCommandParser())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class with name and single command assembly.
        /// </summary>
        /// <param name="name">The sub protocol name.</param>
        /// <param name="commandAssembly">The command assembly.</param>
        public BasicSubProtocol(string name, Assembly commandAssembly)
            : this(name, new List<Assembly> { commandAssembly }, new BasicSubCommandParser())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class with name and command assemblies.
        /// </summary>
        /// <param name="name">The sub protocol name.</param>
        /// <param name="commandAssemblies">The command assemblies.</param>
        public BasicSubProtocol(string name, IEnumerable<Assembly> commandAssemblies)
            : this(name, commandAssemblies, new BasicSubCommandParser())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicSubProtocol&lt;TWebSocketSession&gt;"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="commandAssemblies">The command assemblies.</param>
        /// <param name="requestInfoParser">The request info parser.</param>
        public BasicSubProtocol(string name, IEnumerable<Assembly> commandAssemblies, IRequestInfoParser<SubRequestInfo> requestInfoParser)
            : base(name)
        {
            //The items in commandAssemblies may be null, so filter here
            m_CommandAssemblies.AddRange(commandAssemblies.Where(a => a != null));
            SubRequestParser = requestInfoParser;
        }

        #region ISubProtocol Members

        private void DiscoverCommands()
        {
            var subCommands = new List<ISubCommand<TWebSocketSession>>();

            foreach (var assembly in m_CommandAssemblies)
            {
                subCommands.AddRange(assembly.GetImplementedObjectsByInterface<ISubCommand<TWebSocketSession>>());
            }

#if DEBUG
            var commandNames = subCommands.Select(c => c.Name).ToArray();
            m_Logger.Debug(string.Format("SubProtocol {0} found the commands: [{1}]", this.Name, string.Join(", ", commandNames)));
#endif

            m_CommandDict = new Dictionary<string, ISubCommand<TWebSocketSession>>(subCommands.Count, StringComparer.OrdinalIgnoreCase);

            subCommands.ForEach(c =>
                    {
                        var fc = c as ISubCommandFilterLoader;

                        if (fc != null)
                            fc.LoadSubCommandFilters(m_GlobalFilters);

                        m_CommandDict.Add(c.Name, c);
                    }
                );
        }

        /// <summary>
        /// Initializes with the specified config.
        /// </summary>
        /// <param name="appServer">The app server.</param>
        /// <param name="protocolConfig">The protocol config.</param>
        /// <param name="logger">The logger.</param>
        /// <returns></returns>
        public override bool Initialize(IAppServer appServer, SubProtocolConfig protocolConfig, ILog logger)
        {
            m_Logger = logger;

            var config = appServer.Config;

            m_GlobalFilters = appServer.GetType()
                    .GetCustomAttributes(true)
                    .OfType<SubCommandFilterAttribute>()
                    .Where(a => string.IsNullOrEmpty(a.SubProtocol) || Name.Equals(a.SubProtocol, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (Name.Equals(DefaultName, StringComparison.OrdinalIgnoreCase))
            {
                var commandAssembly = config.Options.GetValue("commandAssembly");

                if (!string.IsNullOrEmpty(commandAssembly))
                {
                    if (!ResolveCommmandAssembly(commandAssembly))
                        return false;
                }
            }

            if (protocolConfig != null && protocolConfig.Commands != null)
            {
                foreach (var commandConfig in protocolConfig.Commands)
                {
                    var assembly = commandConfig.Options.GetValue("assembly");

                    if (!string.IsNullOrEmpty(assembly))
                    {
                        if (!ResolveCommmandAssembly(assembly))
                            return false;
                    }
                }
            }

            //Always discover commands
            DiscoverCommands();

            return true;
        }

        private bool ResolveCommmandAssembly(string definition)
        {
            try
            {
                var assemblies = AssemblyUtil.GetAssembliesFromString(definition);

                if (assemblies.Any())
                    m_CommandAssemblies.AddRange(assemblies);

                return true;
            }
            catch (Exception e)
            {
                m_Logger.Error(e);
                return false;
            }
        }

        /// <summary>
        /// Tries get command from the sub protocol's command inventory.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public override bool TryGetCommand(string name, out ISubCommand<TWebSocketSession> command)
        {
            return m_CommandDict.TryGetValue(name, out command);
        }

        #endregion
    }
}
