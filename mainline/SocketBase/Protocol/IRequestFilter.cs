using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// Request filter interface
    /// </summary>
    /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
    public interface IRequestFilter<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        /// <summary>
        /// Filters received data of the specific session into request info.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset of the current received data in this read buffer.</param>
        /// <param name="length">The length of the current received data.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <param name="left">The left, the length of the data which hasn't been parsed.</param>
        /// <returns></returns>
        TRequestInfo Filter(IAppSession<TRequestInfo> session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left);

        /// <summary>
        /// Gets the size of the left buffer.
        /// </summary>
        /// <value>
        /// The size of the left buffer.
        /// </value>
        int LeftBufferSize { get; }

        /// <summary>
        /// Gets the next request filter.
        /// </summary>
        IRequestFilter<TRequestInfo> NextRequestFilter { get; }
    }
}
