using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.ProtoBase;

namespace SuperSocket.Command
{
    public class CommandMiddleware<TKey, TNetPackageInfo, TPackageInfo, TPackageMapper> : CommandMiddleware<TKey, TNetPackageInfo, TPackageInfo>
        where TPackageInfo : class, IKeyedPackageInfo<TKey>
        where TNetPackageInfo : class
        where TPackageMapper : IPackageMapper<TNetPackageInfo, TPackageInfo>, new()
    {
        public CommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions)
            : base(serviceProvider, commandOptions)
        {
     
        }

        protected override IPackageMapper<TNetPackageInfo, TPackageInfo> CreatePackageMapper(IServiceProvider serviceProvider)
        {
            return new TPackageMapper();
        }
    }

    public class CommandMiddleware<TKey, TPackageInfo> : CommandMiddleware<TKey, TPackageInfo, TPackageInfo>
        where TPackageInfo : class, IKeyedPackageInfo<TKey>
    {

        class TransparentMapper : IPackageMapper<TPackageInfo, TPackageInfo>
        {
            public TPackageInfo Map(TPackageInfo package)
            {
                return package;
            }
        }

        public CommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions)
            : base(serviceProvider, commandOptions)
        {

        }

        protected override IPackageMapper<TPackageInfo, TPackageInfo> CreatePackageMapper(IServiceProvider serviceProvider)
        {
            return new TransparentMapper();
        }
    }

    public class CommandMiddleware<TKey, TNetPackageInfo, TPackageInfo> : MiddlewareBase, IPackageHandler<TNetPackageInfo>
        where TPackageInfo : class, IKeyedPackageInfo<TKey>
        where TNetPackageInfo : class
    {
        private Dictionary<TKey, ICommandSet> _commands;

        protected IPackageMapper<TNetPackageInfo, TPackageInfo> PackageMapper { get; private set; }    

        public CommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions)
        {
            var sessionFactory = serviceProvider.GetService<ISessionFactory>();
            var sessionType = sessionFactory == null ? typeof(IAppSession) : sessionFactory.SessionType;

            var genericTypes = new [] { sessionType, typeof(TPackageInfo)};

            var commandInterface = typeof(ICommand<,>).GetTypeInfo().MakeGenericType(genericTypes);
            var asyncCommandInterface = typeof(IAsyncCommand<,>).GetTypeInfo().MakeGenericType(genericTypes);

            var commandTypes = commandOptions.Value.GetCommandTypes((t) => (commandInterface.IsAssignableFrom(t) || asyncCommandInterface.IsAssignableFrom(t)) && !t.IsAbstract);

            if (sessionType == typeof(IAppSession)) // still support short form command interfaces
            {
                genericTypes = new [] { typeof(TPackageInfo)};
                commandInterface = typeof(ICommand<>).GetTypeInfo().MakeGenericType(genericTypes);
                asyncCommandInterface = typeof(IAsyncCommand<>).GetTypeInfo().MakeGenericType(genericTypes);
                commandTypes = commandTypes.Union(commandOptions.Value.GetCommandTypes((t) => (commandInterface.IsAssignableFrom(t) || asyncCommandInterface.IsAssignableFrom(t)) && !t.IsAbstract));
            }

            var comparer = serviceProvider.GetService<IEqualityComparer<TKey>>();

            var commandSetFactory = ActivatorUtilities.CreateInstance(null, typeof(CommandSetFactory<>).MakeGenericType(typeof(TKey), typeof(TNetPackageInfo), typeof(TPackageInfo), sessionType)) as ICommandSetFactory;
            var commands = commandTypes.Select(t =>  commandSetFactory.Create(serviceProvider, t));

            if (comparer == null)
                _commands = commands.ToDictionary(x => x.Key);
            else
                _commands = commands.ToDictionary(x => x.Key, comparer);

            PackageMapper = CreatePackageMapper(serviceProvider);
        }

        protected virtual IPackageMapper<TNetPackageInfo, TPackageInfo> CreatePackageMapper(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<IPackageMapper<TNetPackageInfo, TPackageInfo>>();
        }

        protected virtual async Task HandlePackage(IAppSession session, TPackageInfo package)
        {
            if (!_commands.TryGetValue(package.Key, out ICommandSet commandSet))
            {
                return;
            }

            await commandSet.ExecuteAsync(session, package);
        }

        protected virtual async Task OnPackageReceived(IAppSession session, TPackageInfo package)
        {
            await HandlePackage(session, package);
        }

        Task IPackageHandler<TNetPackageInfo>.Handle(IAppSession session, TNetPackageInfo package)
        {
            return HandlePackage(session, PackageMapper.Map(package));
        }

        interface ICommandSet
        {
            TKey Key { get; }

            ValueTask ExecuteAsync(IAppSession session, TPackageInfo package);
        }

        interface ICommandSetFactory
        {
            ICommandSet Create(IServiceProvider serviceProvider, Type commandType);
        }

        class CommandSetFactory<TAppSession> : ICommandSetFactory
            where TAppSession : IAppSession
        
        {
            public ICommandSet Create(IServiceProvider serviceProvider, Type commandType)
            {
                var commandSet = new CommandSet<TAppSession>();
                commandSet.Initialize(serviceProvider, commandType);
                return commandSet;
            }
        }

        class CommandSet<TAppSession> : ICommandSet
            where TAppSession : IAppSession
        {
            public IAsyncCommand<TAppSession, TPackageInfo> AsyncCommand { get; private set; }

            public ICommand<TAppSession, TPackageInfo> Command { get; private set; }

            public IReadOnlyList<ICommandFilter> Filters { get; private set; }
            
            public CommandMetadata Metadata { get; private set; }

            public TKey Key { get; private set; }

            private readonly bool _isKeyString = false;

            public CommandSet()
            {
                _isKeyString = typeof(TKey) == typeof(string);
            }

            private CommandMetadata GetCommandMetadata(Type commandType)
            {
                var cmdAtt = commandType.GetCustomAttribute(typeof(CommandAttribute)) as CommandAttribute;
                var cmdMeta = default(CommandMetadata);

                if (cmdAtt == null)
                {
                    if (!_isKeyString)
                    {
                        throw new Exception($"The command {commandType.FullName} needs a CommandAttribute defined.");
                    }

                    cmdMeta = new CommandMetadata(commandType.Name, commandType.Name);
                }
                else
                {
                    var cmdName = cmdAtt.Name;

                    if (string.IsNullOrEmpty(cmdName))
                        cmdName = commandType.Name;

                    if (cmdAtt.Key == null)
                    {
                        if (!_isKeyString)
                        {
                            throw new Exception($"The command {commandType.FullName} needs a Key in type '{typeof(TKey).Name}' defined in its CommandAttribute.");
                        }

                        cmdMeta = new CommandMetadata(cmdName, cmdName);
                    }
                    else
                    {
                        cmdMeta = new CommandMetadata(cmdName, cmdAtt.Key);
                    }
                }

                return cmdMeta;
            }

            public void Initialize(IServiceProvider serviceProvider, Type commandType)
            {
                var command = ActivatorUtilities.CreateInstance(serviceProvider, commandType) as ICommand;                
                var cmdMeta = GetCommandMetadata(commandType);

                try
                {
                    Key = (TKey)cmdMeta.Key;
                }
                catch (Exception e)
                {
                    throw new Exception($"The command {cmdMeta.Name}'s Key {cmdMeta.Key} cannot be converted to the desired type '{typeof(TKey).Name}'.", e);
                }                

                Command = command as ICommand<TAppSession, TPackageInfo>;
                AsyncCommand = command as IAsyncCommand<TAppSession, TPackageInfo>;

                Filters = commandType.GetCustomAttributes(false)
                    .OfType<CommandFilterBaseAttribute>()
                    .OrderBy(f => f.Order)
                    .ToArray();
            }

            public async ValueTask ExecuteAsync(IAppSession session, TPackageInfo package)
            {
                if (Filters.Count > 0)
                {
                    await ExecuteAsyncWithFilter(session, package);
                    return;
                }

                var appSession = (TAppSession)session;

                var asyncCommand = AsyncCommand;

                if (asyncCommand != null)
                {
                    await asyncCommand.ExecuteAsync(appSession, package);
                    return;
                }

                Command.Execute(appSession, package);
            }

            private async ValueTask ExecuteAsyncWithFilter(IAppSession session, TPackageInfo package)
            {
                var context = new CommandExecutingContext();
                context.Package = package;
                context.Session = session;
                context.CurrentCommand = AsyncCommand != null ? (AsyncCommand as ICommand) : (Command as ICommand);

                var filters = Filters;

                var continued = true;

                for (var i = 0; i < filters.Count; i++)
                {
                    var f = filters[i];
                    
                    if (f is AsyncCommandFilterAttribute asyncCommandFilter)
                    {
                        continued = await asyncCommandFilter.OnCommandExecutingAsync(context);
                    }
                    else if (f is CommandFilterAttribute commandFilter)
                    {
                        continued = commandFilter.OnCommandExecuting(context);
                    }

                    if (!continued)
                        break;
                }

                if (!continued)
                    return;                

                try
                {
                    var appSession = (TAppSession)session;
                    var asyncCommand = AsyncCommand;

                    if (asyncCommand != null)
                    {
                        await asyncCommand.ExecuteAsync(appSession, package);
                    }
                    else
                    {
                        Command.Execute(appSession, package);
                    }                    
                }
                catch (Exception e)
                {
                    context.Exception = e;
                }
                finally
                {
                    for (var i = 0; i < filters.Count; i++)
                    {
                        var f = filters[i];
                        
                        if (f is AsyncCommandFilterAttribute asyncCommandFilter)
                        {
                            await asyncCommandFilter.OnCommandExecutedAsync(context);
                        }
                        else if (f is CommandFilterAttribute commandFilter)
                        {
                            commandFilter.OnCommandExecuted(context);
                        }
                    }
                }
            }
        }
    }
}
