using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Channel;

namespace SuperSocket.Command
{
    public class CommandMiddleware<TKey, TPackageInfo> : MiddlewareBase
        where TPackageInfo : class, IKeyedPackageInfo<TKey>
    {
        private Dictionary<TKey, ICommand<TKey>> _commands;

        public CommandMiddleware(IServiceProvider serviceProvider, IOptions<CommandOptions> commandOptions)
        {
            var commandInterface = typeof(ICommand<TKey, TPackageInfo>).GetTypeInfo();
            var asyncCommandInterface = typeof(IAsyncCommand<TKey, TPackageInfo>).GetTypeInfo();            
            var commandTypes = commandOptions.Value.GetCommandTypes((t) => commandInterface.IsAssignableFrom(t) || asyncCommandInterface.IsAssignableFrom(t));

            _commands = commandTypes.Select(t =>  ActivatorUtilities.CreateInstance(serviceProvider, t) as ICommand<TKey>)
                .ToDictionary(x => x.Key);
        }

        public override void Register(IServer server, IAppSession session)
        {
            var channel = session.Channel as IChannel<TPackageInfo>;
            
            if (channel == null)
                throw new Exception("Unmatched package type.");
            
            channel.PackageReceived += async (ch, p) =>
            {
                await OnPackageReceived(session, p);
            };
        }

        private async Task OnPackageReceived(IAppSession session, TPackageInfo package)
        {
            if (!_commands.TryGetValue(package.Key, out ICommand<TKey> command))
            {
                return;
            }

            var asyncCommand = command as IAsyncCommand<TKey, TPackageInfo>;

            if (asyncCommand != null)
            {
                await asyncCommand.ExecuteAsync(null, package);
                return;
            }

            ((ICommand<TKey, TPackageInfo>)command).Execute(null, package);
        }
    }
}
