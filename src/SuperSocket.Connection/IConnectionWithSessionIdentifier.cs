using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Represents a connection that includes a unique session identifier.
    /// </summary>
    public interface IConnectionWithSessionIdentifier
    {
        /// <summary>
        /// Gets the unique identifier for the connection session.
        /// </summary>
        string SessionIdentifier { get; }
    }
}
