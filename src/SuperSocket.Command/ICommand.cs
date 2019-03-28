using System;
using System.Threading.Tasks;

namespace SuperSocket.Command
{
    public interface ICommand<TKey, TPackageInfo>
        where TPackageInfo : IKeyedPackageInfo<TKey>
    {
        TKey Key { get; }

        string Name { get; }

        bool IsAsync { get; }

        void Execute(IAppSession session, TPackageInfo package);

        Task ExecuteAsync(IAppSession session, TPackageInfo package);
    }
}
