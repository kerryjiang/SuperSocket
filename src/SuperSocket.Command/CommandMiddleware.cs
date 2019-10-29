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
        private Dictionary<TKey, ICommand<TKey>> _commands;

        protected IPackageMapper<TNetPackageInfo, TPackageInfo> PackageMapper { get; private set; }    

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

        protected virtual async Task OnPackageReceived(IAppSession session, TPackageInfo package)
        {
            await HandlePackage(session, package);
        }

        Task IPackageHandler<TNetPackageInfo>.Handle(IAppSession session, TNetPackageInfo package)
        {
            return HandlePackage(session, PackageMapper.Map(package));
        }
    }
}
