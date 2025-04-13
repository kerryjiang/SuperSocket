using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Represents a pipe-based connection with input and output capabilities.
    /// </summary>
    public interface IPipeConnection
    {
        /// <summary>
        /// Gets the pipeline filter used for processing data.
        /// </summary>
        IPipelineFilter PipelineFilter { get; }

        /// <summary>
        /// Gets the pipe reader for reading input data.
        /// </summary>
        PipeReader InputReader { get; }

        /// <summary>
        /// Gets the pipe writer for writing output data.
        /// </summary>
        PipeWriter OutputWriter { get; }
    }
}
