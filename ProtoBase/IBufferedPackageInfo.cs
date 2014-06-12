using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The interface for buffered package info
    /// </summary>
    public interface IBufferedPackageInfo
    {
        /// <summary>
        /// Gets the buffered data.
        /// </summary>
        /// <value>
        /// The buffered data.
        /// </value>
        IList<ArraySegment<byte>> Data { get; }
    }
}
