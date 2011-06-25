using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.SocketBase.Protocol
{
    /// <summary>
    /// The main interface you must implement for parsing received data in custom protocol
    /// </summary>
    /// <typeparam name="TCommandInfo">The type of the command info.</typeparam>
    public interface ICommandReader<TCommandInfo>
        where TCommandInfo : ICommandInfo
    {
        /// <summary>
        /// Gets the current app server.
        /// </summary>
        IAppServer AppServer { get; }

        /// <summary>
        /// Finds the command info from current received read buffer.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="readBuffer">The read buffer.</param>
        /// <param name="offset">The offset of the received data in readBuffer.</param>
        /// <param name="length">The length the received data.</param>
        /// <param name="isReusableBuffer">if set to <c>true</c> [is reusable buffer].</param>
        /// <param name="left">The size of left data which has not been parsed by this commandReader.</param>
        /// <returns>
        /// return the found commandInfo, return null if found nothing
        /// </returns>
        TCommandInfo FindCommandInfo(IAppSession session, byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int left);

        /// <summary>
        /// Gets the size of the left buffer.
        /// </summary>
        /// <value>
        /// The size of the left buffer.
        /// </value>
        int LeftBufferSize { get; }

        /// <summary>
        /// Gets the command reader which will be used for next round receiving.
        /// </summary>
        ICommandReader<TCommandInfo> NextCommandReader { get; }
    }
}
