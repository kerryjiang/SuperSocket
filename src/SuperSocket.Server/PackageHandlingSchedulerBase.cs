using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperSocket.Channel;

namespace SuperSocket.Server
{
    public abstract class PackageHandlingSchedulerBase<TPackageInfo> : IPackageHandlingScheduler<TPackageInfo>
    {
        public List<IPackageHandler<TPackageInfo>> PackageHandlers { get; private set; } = new List<IPackageHandler<TPackageInfo>>();

        public Func<IAppSession, PackageHandlingException<TPackageInfo>, ValueTask<bool>> ErrorHandler { get; private set; }

        public abstract ValueTask HandlePackage(IAppSession session, TPackageInfo package);

        public virtual void Initialize(IPackageHandler<TPackageInfo> packageHandler, Func<IAppSession, PackageHandlingException<TPackageInfo>, ValueTask<bool>> errorHandler)
        {
            PackageHandlers.Add(packageHandler);
            PackageHandlers.Distinct();
            ErrorHandler = errorHandler;
        }

        protected async ValueTask HandlePackageInternal(IAppSession session, TPackageInfo package)
        {
            var packageHandlers = PackageHandlers;
            var errorHandler = ErrorHandler;

            try
            {
                if (packageHandlers.Count > 0)
                    foreach (var item in packageHandlers)// TODO consider whether async is required
                    {
                        await item.Handle(session, package);
                    }
            }
            catch (Exception e)
            {
                var toClose = await errorHandler(session, new PackageHandlingException<TPackageInfo>($"Session {session.SessionID} got an error when handle a package.", package, e));

                if (toClose)
                {
                    session.CloseAsync(CloseReason.ApplicationError).DoNotAwait();
                }
            }
        }
    }
}