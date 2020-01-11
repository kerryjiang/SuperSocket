using System;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Command
{
    public interface ICommand
    {
        string Name { get; }
    }

    public interface ICommand<TKey> : ICommand
    {
        TKey Key { get; }
    }

    public interface ICommand<TKey, TPackageInfo> : ICommand<TKey, IAppSession, TPackageInfo>
    {

    }

    public interface ICommand<TKey, TAppSession, TPackageInfo> : ICommand<TKey>
        where TAppSession : IAppSession
    {
        void Execute(TAppSession session, TPackageInfo package);
    }

    public interface IAsyncCommand<TKey, TPackageInfo> : IAsyncCommand<TKey, IAppSession, TPackageInfo>
    {

    }

    public interface IAsyncCommand<TKey, TAppSession, TPackageInfo> : ICommand<TKey>
        where TAppSession : IAppSession
    {
        ValueTask ExecuteAsync(TAppSession session, TPackageInfo package);
    }
}
