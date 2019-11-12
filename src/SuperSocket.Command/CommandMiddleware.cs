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
        private Dictionary<TKey, CommandSet> _commands;

        protected IPackageMapper<TNetPackageInfo, TPackageInfo> PackageMapper { get; private set; }    

        public CommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions)
        {
            var commandInterface = typeof(ICommand<TKey, TPackageInfo>).GetTypeInfo();
            var asyncCommandInterface = typeof(IAsyncCommand<TKey, TPackageInfo>).GetTypeInfo();            
            var commandTypes = commandOptions.Value.GetCommandTypes((t) => commandInterface.IsAssignableFrom(t) || asyncCommandInterface.IsAssignableFrom(t));
            var comparer = serviceProvider.GetService<IEqualityComparer<TKey>>();

            var commands = commandTypes.Select(t =>  new CommandSet(serviceProvider, t));

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

        public override void Register(IServer server, IAppSession session)
        {

        }

        protected virtual async Task HandlePackage(IAppSession session, TPackageInfo package)
        {
            if (!_commands.TryGetValue(package.Key, out CommandSet commandSet))
            {
                return;
            }

            var asyncCommand = commandSet.AsyncCommand;

            if (asyncCommand != null)
            {
                await asyncCommand.ExecuteAsync(session, package);
                return;
            }

            commandSet.Command.Execute(session, package);
        }

        protected virtual async Task OnPackageReceived(IAppSession session, TPackageInfo package)
        {
            await HandlePackage(session, package);
        }

        Task IPackageHandler<TNetPackageInfo>.Handle(IAppSession session, TNetPackageInfo package)
        {
            return HandlePackage(session, PackageMapper.Map(package));
        }

        class CommandSet
        {
            public IAsyncCommand<TKey, TPackageInfo> AsyncCommand { get; private set; }

            public ICommand<TKey, TPackageInfo> Command { get; private set; }

            public IReadOnlyList<ICommandFilter> Filters { get; private set; }

            public TKey Key
            {
                get
                {
                    return Command.Key;
                }
            }

            public CommandSet(IServiceProvider serviceProvider, Type commandType)
            {
                Command = ActivatorUtilities.CreateInstance(serviceProvider, commandType) as ICommand<TKey, TPackageInfo>;
                AsyncCommand = Command as IAsyncCommand<TKey, TPackageInfo>;

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

                var asyncCommand = AsyncCommand;

                if (asyncCommand != null)
                {
                    await asyncCommand.ExecuteAsync(session, package);
                    return;
                }

                Command.Execute(session, package);
            }

            private async ValueTask ExecuteAsyncWithFilter(IAppSession session, TPackageInfo package)
            {
                var context = new CommandExecutingContext();
                context.Package = package;
                context.Session = session;
                context.CurrentCommand = AsyncCommand != null ? (AsyncCommand as ICommand) : (Command as ICommand);

                var filters = Filters;

                var cancelled = false;

                for (var i = 0; i < filters.Count; i++)
                {
                    var f = filters[i];
                    
                    if (f is AsyncCommandFilterAttribute asyncCommandFilter)
                    {
                        cancelled = await asyncCommandFilter.OnCommandExecutingAsync(context);
                    }
                    else if (f is CommandFilterAttribute commandFilter)
                    {
                        cancelled = commandFilter.OnCommandExecuting(context);
                    }

                    if (cancelled)
                        break;
                }

                if (cancelled)
                    return;

                try
                {
                    var asyncCommand = AsyncCommand;

                    if (asyncCommand != null)
                    {
                        await asyncCommand.ExecuteAsync(session, package);
                    }
                    else
                    {
                        Command.Execute(session, package);
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
