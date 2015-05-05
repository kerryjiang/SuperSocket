using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The interface for the composite target
    /// </summary>
    public interface ICompositeTarget
    {
        /// <summary>
        /// Resolves the specified application server.
        /// </summary>
        /// <param name="appServer">The application server.</param>
        /// <param name="exportProvider">The export provider.</param>
        /// <returns></returns>
        bool Resolve(IAppServer appServer, ExportProvider exportProvider);
    }
}
