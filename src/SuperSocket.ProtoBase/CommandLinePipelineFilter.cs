using System;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// A pipeline filter that processes command-line-style input terminated by a line break (\r\n).
    /// </summary>
    public class CommandLinePipelineFilter : TerminatorPipelineFilter<StringPackageInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLinePipelineFilter"/> class.
        /// </summary>
        public CommandLinePipelineFilter()
            : base(new[] { (byte)'\r', (byte)'\n' })
        {
        }
    }
}
