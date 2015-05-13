using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Provider;

namespace SuperSocket.SocketBase.CompositeTargets
{
    /// <summary>
    /// The basic class for the signle element result composite target
    /// </summary>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    public abstract class SingleResultCompositeTarget<TTarget> : SingleResultCompositeTarget<TTarget, IProviderMetadata>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleResultCompositeTarget{TTarget}"/> class.
        /// </summary>
        /// <param name="configSelector">The configuration selector.</param>
        /// <param name="callback">The callback which will be invoked after the resolving is finished successfully.</param>
        /// <param name="required">if set to <c>true</c> [required], indicating whether this resolving result is required.</param>
        protected SingleResultCompositeTarget(Func<IServerConfig, string> configSelector, Action<TTarget> callback, bool required)
            : base(configSelector, callback, required)
        {

        }
    }

    /// <summary>
    /// The basic class for the signle element result composite target
    /// </summary>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
    public abstract class SingleResultCompositeTarget<TTarget, TMetadata> : SingleResultCompositeTargetCore<TTarget, TMetadata>
        where TMetadata : IProviderMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleResultCompositeTarget{TTarget, TMetadata}"/> class.
        /// </summary>
        /// <param name="configSelector">The configuration selector.</param>
        /// <param name="callback">The callback which will be invoked after the resolving is finished successfully.</param>
        /// <param name="required">if set to <c>true</c> [required], indicating whether this resolving result is required.</param>
        protected SingleResultCompositeTarget(Func<IServerConfig, string> configSelector, Action<TTarget> callback, bool required)
            : base(configSelector, callback, required)
        {
            
        }

        /// <summary>
        /// Match the metadata by the name.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        protected override bool MetadataNameEqual(TMetadata metadata, string name)
        {
            return metadata.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// The basic class for the signle element result composite target
    /// </summary>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
    public abstract class SingleResultCompositeTargetCore<TTarget, TMetadata> : CompositeTargetBase<TTarget>
    {
        /// <summary>
        /// Gets a value indicating whether this resolving result is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        protected bool Required { get; private set; }

        /// <summary>
        /// Gets the configuration selector.
        /// </summary>
        /// <value>
        /// The configuration selector.
        /// </value>
        protected Func<IServerConfig, string> ConfigSelector { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleResultCompositeTargetCore{TTarget, TMetadata}"/> class.
        /// </summary>
        /// <param name="configSelector">The configuration selector.</param>
        /// <param name="callback">The callback which will be invoked after the resolving is finished successfully.</param>
        /// <param name="required">if set to <c>true</c> [required], indicating whether this resolving result is required.</param>
        protected SingleResultCompositeTargetCore(Func<IServerConfig, string> configSelector, Action<TTarget> callback, bool required)
            : base(callback)
        {
            ConfigSelector = configSelector;
            Required = required;
        }

        /// <summary>
        /// Tries to resolve.
        /// </summary>
        /// <param name="appServer">The application server.</param>
        /// <param name="exportProvider">The export provider.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        protected override bool TryResolve(IAppServer appServer, ExportProvider exportProvider, out TTarget result)
        {
            result = default(TTarget);

            var config = appServer.Config;
            var logger = appServer.Logger;

            var factories = exportProvider.GetExports<TTarget, TMetadata>();

            if (Required)
            {
                if (factories == null || !factories.Any())
                {
                    logger.ErrorFormat("No factory for {0} defined.", typeof(TTarget).FullName);
                    return false;
                }
            }

            var configValue = ConfigSelector(config);

            var lazyFactory = Sort(factories).FirstOrDefault(
                f => string.IsNullOrEmpty(configValue) || MetadataNameEqual(f.Metadata, configValue));

            if (lazyFactory == null)
            {
                if(Required)
                {
                    logger.ErrorFormat("No available factory of {0} was not found!", typeof(TTarget).FullName);
                    return false;
                }

                return true;
            }

            result = lazyFactory.Value;
            return PrepareResult(lazyFactory.Value, appServer, lazyFactory.Metadata);
        }

        /// <summary>
        /// Sorts the specified factories by priority.
        /// </summary>
        /// <param name="factories">The factories.</param>
        /// <returns></returns>
        protected virtual IEnumerable<Lazy<TTarget, TMetadata>> Sort(IEnumerable<Lazy<TTarget, TMetadata>> factories)
        {
            return factories;
        }

        /// <summary>
        /// Match the metadata by the name.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        protected abstract bool MetadataNameEqual(TMetadata metadata, string name);

        /// <summary>
        /// Prepares the result before it is returned.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="appServer">The application server.</param>
        /// <param name="metadata">The metadata.</param>
        /// <returns></returns>
        protected virtual bool PrepareResult(TTarget result, IAppServer appServer, TMetadata metadata)
        {
            return true;
        }
    }
}
