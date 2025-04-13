using System;
using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// A pipeline filter that processes text packages terminated by a specific byte sequence.
    /// </summary>
    public class TerminatorTextPipelineFilter : TerminatorPipelineFilter<TextPackageInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminatorTextPipelineFilter"/> class with the specified terminator.
        /// </summary>
        /// <param name="terminator">The byte sequence that marks the end of a package.</param>
        public TerminatorTextPipelineFilter(ReadOnlyMemory<byte> terminator)
            : base(terminator)
        {
        }

        /// <summary>
        /// Decodes a text package from the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the text package data.</param>
        /// <returns>The decoded <see cref="TextPackageInfo"/>.</returns>
        protected override TextPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            return new TextPackageInfo { Text = buffer.GetString(Encoding.UTF8) };
        }
    }
}
