using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.SocketBase.CompositeTargets
{
    /// <summary>
    /// the basic class of composite target
    /// </summary>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    public abstract class CompositeTargetBase<TTarget> : ICompositeTarget
    {
        private Action<TTarget> m_Callback;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeTargetBase{TTarget}"/> class.
        /// </summary>
        /// <param name="callback">The callback which will be invoked after the resolving is finished successfully.</param>
        protected CompositeTargetBase(Action<TTarget> callback)
        {
            m_Callback = callback;
        }

        /// <summary>
        /// Resolves the specified application server.
        /// </summary>
        /// <param name="appServer">The application server.</param>
        /// <param name="exportProvider">The export provider.</param>
        /// <returns></returns>
        public bool Resolve(IAppServer appServer, ExportProvider exportProvider)
        {
            TTarget result = default(TTarget);

            if (!TryResolve(appServer, exportProvider, out result))
                return false;

            m_Callback(result);
            return true;
        }

        /// <summary>
        /// Tries to resolve.
        /// </summary>
        /// <param name="appServer">The application server.</param>
        /// <param name="exportProvider">The export provider.</param>
        /// <param name="result">The resolving result.</param>
        /// <returns></returns>
        protected abstract bool TryResolve(IAppServer appServer, ExportProvider exportProvider, out TTarget result);
    }
}
