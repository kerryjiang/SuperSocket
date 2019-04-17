using System;
using System.Threading.Tasks;

namespace SuperSocket.Command
{
    public interface ICommand<TKey>
    {
        TKey Key { get; }

        string Name { get; }
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
