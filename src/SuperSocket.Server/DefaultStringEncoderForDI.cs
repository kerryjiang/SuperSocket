using System;
using System.IO.Pipelines;
using System.Text;
using Microsoft.Extensions.Options;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions;

namespace SuperSocket
{
    /// <summary>
    /// Provides a default string encoder for dependency injection, using server options for configuration.
    /// </summary>
    class DefaultStringEncoderForDI : DefaultStringEncoder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStringEncoderForDI"/> class with the specified server options.
        /// </summary>
        /// <param name="serverOptions">The server options containing the default text encoding.</param>
        public DefaultStringEncoderForDI(IOptions<ServerOptions> serverOptions)
            : base(serverOptions.Value?.DefaultTextEncoding ?? new UTF8Encoding(false))
        {
        }
    }
}