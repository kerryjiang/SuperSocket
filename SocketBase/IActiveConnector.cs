using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// Active connect result model
    /// </summary>
    public class ActiveConnectResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the conecting is sucessfull
        /// </summary>
        /// <value>
        ///   <c>true</c> if result; otherwise, <c>false</c>.
        /// </value>
        public bool Result { get; set; }

        /// <summary>
        /// Gets or sets the connected session.
        /// </summary>
        /// <value>
        /// The connected session.
        /// </value>
        public IAppSession Session { get; set; }
    }

    /// <summary>
    /// The inerface to connect the remote endpoint actively
    /// </summary>
    public interface IActiveConnector
    {
        /// <summary>
        /// Connect the target endpoint actively.
        /// </summary>
        /// <param name="targetEndPoint">The target end point.</param>
        /// <returns></returns>
        Task<ActiveConnectResult> ActiveConnect(EndPoint targetEndPoint);
    }
}
