using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// The pipeline data processor
    /// </summary>
    public interface IPipelineProcessor
    {
        /// <summary>
        /// Processes the input segment.
        /// </summary>
        /// <param name="segment">The input segment.</param>
        /// <param name="state">The buffer state.</param>
        /// <returns>the processing result</returns>
        ProcessResult Process(ArraySegment<byte> segment, IBufferState state);

        /// <summary>
        /// Gets the received cache.
        /// </summary>
        /// <value>
        /// The cache.
        /// </value>
        BufferList Cache { get; }
    }
}
