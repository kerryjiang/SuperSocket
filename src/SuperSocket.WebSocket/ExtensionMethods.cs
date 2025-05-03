using System;
using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// Extension methods for WebSocket implementation.
    /// </summary>
    public static partial class ExtensionMethods
    {
        private readonly static char[] m_CrCf = new char[] { '\r', '\n' };

        /// <summary>
        /// Appends in the format with CrCf as suffix.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="format">The format.</param>
        /// <param name="arg">The arg.</param>
        public static void AppendFormatWithCrCf(this StringBuilder builder, string format, object arg)
        {
            builder.AppendFormat(format, arg);
            builder.Append(m_CrCf);
        }

        /// <summary>
        /// Appends in the format with CrCf as suffix.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public static void AppendFormatWithCrCf(this StringBuilder builder, string format, params object[] args)
        {
            builder.AppendFormat(format, args);
            builder.Append(m_CrCf);
        }

        /// <summary>
        /// Appends with CrCf as suffix.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="content">The content.</param>
        public static void AppendWithCrCf(this StringBuilder builder, string content)
        {
            builder.Append(content);
            builder.Append(m_CrCf);
        }

        /// <summary>
        /// Appends with CrCf as suffix.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static void AppendWithCrCf(this StringBuilder builder)
        {
            builder.Append(m_CrCf);
        }

        /// <summary>
        /// Creates a copy of the given ReadOnlySequence.
        /// </summary>
        /// <param name="seq">The sequence to copy.</param>
        /// <returns>A new ReadOnlySequence that is a copy of the input sequence.</returns>
        internal static ReadOnlySequence<byte> CopySequence(ref this ReadOnlySequence<byte> seq)
        {
            SequenceSegment head = null;
            SequenceSegment tail = null;

            foreach (var segment in seq)
            {                
                var newSegment = SequenceSegment.CopyFrom(segment);

                if (head == null)
                    tail = head = newSegment;
                else
                    tail = tail.SetNext(newSegment);
            }

            return new ReadOnlySequence<byte>(head, 0, tail, tail.Memory.Length);
        }

        /// <summary>
        /// Destructs the given ReadOnlySequence into its head and tail segments.
        /// </summary>
        /// <param name="first">The sequence to destruct.</param>
        /// <returns>A tuple containing the head and tail segments of the sequence.</returns>
        internal static (SequenceSegment, SequenceSegment) DestructSequence(ref this ReadOnlySequence<byte> first)
        {
            SequenceSegment head = first.Start.GetObject() as SequenceSegment;
            SequenceSegment tail = first.End.GetObject() as SequenceSegment;
            
            if (head == null)
            {
                foreach (var segment in first)
                {                
                    if (head == null)
                        tail = head = new SequenceSegment(segment);
                    else
                        tail = tail.SetNext(new SequenceSegment(segment));
                }
            }

            return (head, tail);
        }

        /// <summary>
        /// Concatenates two ReadOnlySequences into a single sequence.
        /// </summary>
        /// <param name="first">The first sequence.</param>
        /// <param name="second">The second sequence.</param>
        /// <returns>A new ReadOnlySequence that is the concatenation of the two input sequences.</returns>
        internal static ReadOnlySequence<byte> ConcatSequence(ref this ReadOnlySequence<byte> first, ref ReadOnlySequence<byte> second)
        {
            var (head, tail) = first.DestructSequence();

            if (!second.IsEmpty)
            {
                foreach (var segment in second)
                {
                    tail = tail.SetNext(new SequenceSegment(segment));
                }
            }

            return new ReadOnlySequence<byte>(head, 0, tail, tail.Memory.Length);
        }

        /// <summary>
        /// Concatenates a ReadOnlySequence with a single SequenceSegment.
        /// </summary>
        /// <param name="first">The sequence to concatenate.</param>
        /// <param name="segment">The segment to append to the sequence.</param>
        /// <returns>A new ReadOnlySequence that includes the appended segment.</returns>
        internal static ReadOnlySequence<byte> ConcatSequence(ref this ReadOnlySequence<byte> first, SequenceSegment segment)
        {
            var (head, tail) = first.DestructSequence();
            tail = tail.SetNext(segment);
            return new ReadOnlySequence<byte>(head, 0, tail, tail.Memory.Length);
        }
    }
}
