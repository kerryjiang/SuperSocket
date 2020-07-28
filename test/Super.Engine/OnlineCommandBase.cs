using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SuperSocket.Command;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AutoMapper;
using Volo.Abp.DependencyInjection;

namespace Super.Engine
{
    [ExposeServices(typeof(IAsyncCommand<IOnlineSession, OnlinePackageInfo>))]
    public abstract class OnlineCommandBase : IAsyncCommand<IOnlineSession, OnlinePackageInfo>, ISingletonDependency
    {      
        public IServiceProvider ServiceProvider { get; set; }
      
        public ILogger Logger => ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType().Name);
      
        public abstract ValueTask ExecuteAsync(IOnlineSession session, OnlinePackageInfo package);
      
        protected virtual ValueTask Reply<TResponse>(IOnlineSession session, OnlinePackageInfo package, TResponse response)
            => Reply(session, packageOptions =>
            {
                packageOptions.Key = package.Key;
                packageOptions.RequestID = package.RequestID;
                packageOptions.Object = response;
            });
      
        protected virtual ValueTask Reply(IOnlineSession session, Action<OnlinePackageInfo> actionResponsePackage)
        {
            var responsePackage = new OnlinePackageInfo();
            actionResponsePackage?.Invoke(responsePackage);

            return session.SendAsync(responsePackage);
        }
    }
  
    public abstract class OnlineCommandBase<T> : OnlineCommandBase
    {     
        public IMapperAccessor MapperAccessor { get; set; }
      
        public OnlineCommandBase()
            : base()
        {
            var commandAttribyte = GetType().GetCustomAttribute<CommandAttribute>(true);

            Check.NotNull(commandAttribyte, nameof(CommandAttribute), nameof(OnlineCommandBase<T>));
            Check.NotNull(commandAttribyte.Key, nameof(commandAttribyte.Key));

            if (commandAttribyte.Key is short key)
            {
                OnlinePackageDecoderFactory.RegisterFactory(key, new DecoderFactory<T>());
                return;
            }

            throw new InvalidCastException(nameof(commandAttribyte.Key));
        }
        
        public abstract ValueTask ExecuteAsync(IOnlineSession session, OnlinePackageInfo<T> package);
      
        public override ValueTask ExecuteAsync(IOnlineSession session, OnlinePackageInfo package)
            => ExecuteAsync(session, MapperAccessor.Mapper.Map<OnlinePackageInfo, OnlinePackageInfo<T>>(package));
    }
}
