using System.Buffers;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// A pipeline filter that decodes text packages terminated by a line break (\r\n).
    /// </summary>
    public class LinePipelineFilter : TerminatorPipelineFilter<TextPackageInfo>
    {
        /// <summary>
        /// Gets the encoding used for decoding text packages.
        /// </summary>
        protected Encoding Encoding { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinePipelineFilter"/> class with UTF-8 encoding.
        /// </summary>
        public LinePipelineFilter()
            : this(Encoding.UTF8)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinePipelineFilter"/> class with the specified encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use for decoding text packages.</param>
        public LinePipelineFilter(Encoding encoding)
            : base(new[] { (byte)'\r', (byte)'\n' })
        {
            Encoding = encoding;
        }

        /// <summary>
        /// Decodes a text package from the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing the text package data.</param>
        /// <returns>The decoded <see cref="TextPackageInfo"/>.</returns>
        protected override TextPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer)
        {
            return new TextPackageInfo { Text = buffer.GetString(Encoding) };
        }
    }
}
