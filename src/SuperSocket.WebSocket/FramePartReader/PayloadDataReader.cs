using System;
using System.Buffers;
using System.Text;
using SuperSocket.ProtoBase;

namespace SuperSocket.WebSocket.FramePartReader
{
    /// <summary>
    /// Represents a reader for processing the payload data of a WebSocket package.
    /// </summary>
    class PayloadDataReader : PackagePartReader
    {
        /// <summary>
        /// Processes the payload data of a WebSocket package.
        /// </summary>
        /// <param name="package">The WebSocket package being processed.</param>
        /// <param name="filterContext">The context of the pipeline filter.</param>
        /// <param name="reader">The sequence reader for the payload data.</param>
        /// <param name="nextPartReader">The next part reader to process subsequent fragments.</param>
        /// <param name="needMoreData">Indicates whether more data is needed to complete processing.</param>
        /// <returns>True if the payload data is fully processed; otherwise, false.</returns>
        public override bool Process(WebSocketPackage package, object filterContext, ref SequenceReader<byte> reader, out IPackagePartReader<WebSocketPackage> nextPartReader, out bool needMoreData)
        {
            nextPartReader = null;

            long required = package.PayloadLength;

            if (reader.Remaining < required)
            {
                needMoreData = true;
                return false;
            }

            needMoreData = false;

            var seq = reader.Sequence.Slice(reader.Consumed, required);

            if (package.HasMask)
                DecodeMask(ref seq, package.MaskKey);

            try
            {
                // single fragment
                if (package.FIN && package.Head == null)
                {
                    package.Data = seq;
                }
                else
                {
                    package.ConcatSequence(ref seq);
                }

                if (package.FIN)
                {
                    if (package.Head != null)
                    {
                        package.BuildData();
                    }

                    var websocketFilterContext = filterContext as WebSocketPipelineFilterContext;

                    if (websocketFilterContext != null && websocketFilterContext.Extensions != null && websocketFilterContext.Extensions.Count > 0)
                    {
                        foreach (var extension in websocketFilterContext.Extensions)
                        {
                            try
                            {
                                extension.Decode(package);
                            }
                            catch (Exception e)
                            {
                                throw new Exception($"Problem happened when decode with the extension {extension.Name}.", e);
                            }
                        }
                    }

                    var data = package.Data;

                    if (package.OpCode == OpCode.Text)
                    {
                        package.Message = data.GetString(Encoding.UTF8);
                        package.Data = default;
                    }
                    else
                    {
                        package.Data = data.CopySequence();
                    }
                    
                    return true;
                }
                else
                {
                    // start to process next fragment
                    nextPartReader = FixPartReader;
                    return false;
                }
            }
            finally
            {
                reader.Advance(required);
            }
        }

        /// <summary>
        /// Decodes the masked payload data of a WebSocket package.
        /// </summary>
        /// <param name="sequence">The sequence of bytes to decode.</param>
        /// <param name="mask">The masking key used for decoding.</param>
        internal unsafe void DecodeMask(ref ReadOnlySequence<byte> sequence, byte[] mask)
        {
            var index = 0;
            var maskLen = mask.Length;

            foreach (var piece in sequence)
            {
                fixed (byte* ptr = &piece.Span.GetPinnableReference())
                {
                    var span = new Span<byte>(ptr, piece.Span.Length);

                    for (var i = 0; i < span.Length; i++)
                    {
                        span[i] = (byte)(span[i] ^ mask[index++ % maskLen]);
                    }
                }
            }
        }
    }
}
