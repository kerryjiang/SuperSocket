using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Request information interface
    /// </summary>
    public interface IRequestInfo
    {
        /// <summary>
        /// Gets the key of this request.
        /// </summary>
        string Key { get; }
    }

    /// <summary>
    /// Request information interface
    /// </summary>
    /// <typeparam name="TRequestData">The type of the request data.</typeparam>
    public interface IRequestInfo<TRequestData> : IRequestInfo
    {
        /// <summary>
        /// Gets the data of this request.
        /// </summary>
        TRequestData Data { get; }
    }
}
