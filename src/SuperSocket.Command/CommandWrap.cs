using System;
using System.Threading.Tasks;
using System.Text.Json;
using SuperSocket.ProtoBase;
using Microsoft.Extensions.DependencyInjection;

namespace SuperSocket.Command
{
    interface ICommandWrap
    {
        ICommand InnerCommand { get; }
    }

    class CommandWrap<TAppSession, TPackageInfo, IPackageInterface, TCommand> : ICommand<TAppSession, TPackageInfo>, ICommandWrap
        where TAppSession : IAppSession
        where TPackageInfo : IPackageInterface
        where TCommand : ICommand<TAppSession, IPackageInterface>
    {
        public TCommand InnerCommand { get; }

        public CommandWrap(TCommand command)
        {
            InnerCommand = command;
        }

        public CommandWrap(IServiceProvider serviceProvider)
        {
            InnerCommand = (TCommand)ActivatorUtilities.CreateInstance(serviceProvider, typeof(TCommand));
        }

        public void Execute(TAppSession session, TPackageInfo package)
        {
            InnerCommand.Execute(session, package);
        }

        ICommand ICommandWrap.InnerCommand
        {
            get { return InnerCommand; }
        }
    }

    class AsyncCommandWrap<TAppSession, TPackageInfo, IPackageInterface, TAsyncCommand> : IAsyncCommand<TAppSession, TPackageInfo>, ICommandWrap
        where TAppSession : IAppSession
        where TPackageInfo : IPackageInterface
        where TAsyncCommand : IAsyncCommand<TAppSession, IPackageInterface>
    {
        public TAsyncCommand InnerCommand { get; }

        public AsyncCommandWrap(TAsyncCommand command)
        {
            InnerCommand = command;
        }

        public AsyncCommandWrap(IServiceProvider serviceProvider)
        {
            InnerCommand = (TAsyncCommand)ActivatorUtilities.CreateInstance(serviceProvider, typeof(TAsyncCommand));
        }

        public async ValueTask ExecuteAsync(TAppSession session, TPackageInfo package)
        {
            await InnerCommand.ExecuteAsync(session, package);
        }

        ICommand ICommandWrap.InnerCommand
        {
            get { return InnerCommand; }
        }
    }
}
