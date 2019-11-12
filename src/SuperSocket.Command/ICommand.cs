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

    public interface ICommand<TKey, TPackageInfo> : ICommand<TKey>
        where TPackageInfo : IKeyedPackageInfo<TKey>
    {
        void Execute(IAppSession session, TPackageInfo package);
    }

    public interface IAsyncCommand<TKey, TPackageInfo> : ICommand<TKey>
        where TPackageInfo : IKeyedPackageInfo<TKey>
    {
        Task ExecuteAsync(IAppSession session, TPackageInfo package);
    }
}
