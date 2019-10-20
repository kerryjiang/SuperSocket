using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket;
using SuperSocket.Channel;
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
            PackageMapper = new TPackageMapper();
        }
    }

    public class CommandMiddleware<TKey, TNetPackageInfo, TPackageInfo> : CommandMiddleware<TKey, TPackageInfo>
        where TPackageInfo : class, IKeyedPackageInfo<TKey>
        where TNetPackageInfo : class
    {

        protected IPackageMapper<TNetPackageInfo, TPackageInfo> PackageMapper { get; set; }

        public CommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions)
            : base(serviceProvider, commandOptions)
        {
            PackageMapper = serviceProvider.GetService<IPackageMapper<TNetPackageInfo, TPackageInfo>>();
        }

        protected async Task OnPackageReceived(IAppSession session, TNetPackageInfo package)
        {
            await base.OnPackageReceived(session, PackageMapper.Map(package));
        }
    }

    public class CommandMiddleware<TKey, TPackageInfo> : MiddlewareBase, IPackageHandler<TPackageInfo>
        where TPackageInfo : class, IKeyedPackageInfo<TKey>
    {
        private Dictionary<TKey, ICommand<TKey>> _commands;        

        public CommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions)
        {
            var commandInterface = typeof(ICommand<TKey, TPackageInfo>).GetTypeInfo();
            var asyncCommandInterface = typeof(IAsyncCommand<TKey, TPackageInfo>).GetTypeInfo();            
            var commandTypes = commandOptions.Value.GetCommandTypes((t) => commandInterface.IsAssignableFrom(t) || asyncCommandInterface.IsAssignableFrom(t));
            var comparer = serviceProvider.GetService<IEqualityComparer<TKey>>();

            var commands = commandTypes.Select(t =>  ActivatorUtilities.CreateInstance(serviceProvider, t) as ICommand<TKey>);

            if (comparer == null)
                _commands = commands.ToDictionary(x => x.Key);
            else
                _commands = commands.ToDictionary(x => x.Key, comparer);
        }

        public override void Register(IServer server, IAppSession session)
        {

        }

        protected async Task OnPackageReceived(IAppSession session, TPackageInfo package)
        {
            if (!_commands.TryGetValue(package.Key, out ICommand<TKey> command))
            {
                return;
            }

            var asyncCommand = command as IAsyncCommand<TKey, TPackageInfo>;

            if (asyncCommand != null)
            {
                await asyncCommand.ExecuteAsync(session, package);
                return;
            }

            ((ICommand<TKey, TPackageInfo>)command).Execute(session, package);
        }

        Task IPackageHandler<TPackageInfo>.Handle(IAppSession session, TPackageInfo package)
        {
            return OnPackageReceived(session, package);
        }
    }
}
