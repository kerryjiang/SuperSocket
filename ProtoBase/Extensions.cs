using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// Extentions class
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets string from the binary segments data.
        /// </summary>
        /// <param name="encoding">The text encoding to decode the binary data.</param>
        /// <param name="data">The binary segments data.</param>
        /// <returns>the decoded string</returns>
        public static string GetString(this Encoding encoding, IList<ArraySegment<byte>> data)
        {
            var total = data.Sum(x => x.Count);

            var output = new char[encoding.GetMaxCharCount(total)];

            var decoder = encoding.GetDecoder();

            var totalCharsLen = 0;
            var lastIndex = data.Count - 1;
            var bytesUsed = 0;
            var charsUsed = 0;
            var completed = false;

            for (var i = 0; i < data.Count; i++)
            {
                var segment = data[i];

                decoder.Convert(segment.Array, segment.Offset, segment.Count, output, totalCharsLen, output.Length - totalCharsLen, i == lastIndex, out bytesUsed, out charsUsed, out completed);
                totalCharsLen += charsUsed;
            }

            return new string(output, 0, totalCharsLen);
        }

        /// <summary>
        /// Gets string from the binary segments data.
        /// </summary>
        /// <param name="encoding">The text encoding to decode the binary data.</param>
        /// <param name="data">The binary segments data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>
        /// the decoded string
        /// </returns>
        public static string GetString(this Encoding encoding, IList<ArraySegment<byte>> data, int offset, int length)
        {
            var output = new char[encoding.GetMaxCharCount(length)];

            var decoder = encoding.GetDecoder();

            var totalCharsLen = 0;
            var totalBytesLen = 0;
            var lastIndex = data.Count - 1;
            var bytesUsed = 0;
            var charsUsed = 0;
            var completed = false;

            var targetOffset = 0;

            for (var i = 0; i < data.Count; i++)
            {
                var segment = data[i];
                var srcOffset = segment.Offset;
                var srcLength = segment.Count;
                var lastSegment = false;

                //Haven't found the offset position
                if (totalBytesLen == 0)
                {
                    var targetEndOffset = targetOffset + segment.Count - 1;

                    if (offset > targetEndOffset)
                    {
                        targetOffset = targetEndOffset + 1;
                        continue;
                    }

                    //the offset locates in this segment
                    var margin = offset - targetOffset;
                    srcOffset = srcOffset + margin;
                    srcLength = srcLength - margin;

                    if (srcLength >= length)
                    {
                        srcLength = length;
                        lastSegment = true;
                    }
                }
                else
                {
                    var restLength = length - totalBytesLen;

                    if (restLength <= srcLength)
                    {
                        srcLength = restLength;
                        lastSegment = true;
                    }
                }

                decoder.Convert(segment.Array, srcOffset, srcLength, output, totalCharsLen, output.Length - totalCharsLen, lastSegment, out bytesUsed, out charsUsed, out completed);
                totalCharsLen += charsUsed;
                totalBytesLen += bytesUsed;
            }

            return new string(output, 0, totalCharsLen);
        }

        /// <summary>
        /// Gets a buffer reader instance which can be reused.
        /// </summary>
        /// <typeparam name="TPackageInfo">The type of the package info.</typeparam>
        /// <param name="receiveFilter">The receive filter.</param>
        /// <param name="data">The buffer data source.</param>
        /// <returns></returns>
        public static IBufferReader GetBufferReader<TPackageInfo>(this IReceiveFilter<TPackageInfo> receiveFilter, IList<ArraySegment<byte>> data)
            where TPackageInfo : IPackageInfo
        {
            return GetBufferReader<BufferListReader, TPackageInfo>(receiveFilter, data);
        }

        /// <summary>
        /// Gets a buffer reader instance which can be reused.
        /// </summary>
        /// <typeparam name="TReader">The type of the reader.</typeparam>
        /// <typeparam name="TPackageInfo">The type of the package info.</typeparam>
        /// <param name="receiveFilter">The receive filter.</param>
        /// <param name="data">The buffer data source.</param>
        /// <returns></returns>
        public static IBufferReader GetBufferReader<TReader, TPackageInfo>(this IReceiveFilter<TPackageInfo> receiveFilter, IList<ArraySegment<byte>> data)
            where TReader : BufferListReader, new()
            where TPackageInfo : IPackageInfo
        {
            //var reader = BufferListReader.GetCurrent<TReader>(); // don't use thread context for BufferListReader for now
            var reader = new BufferListReader();
            reader.Initialize(data);
            return reader;
        }

        /// <summary>
        /// Gets the buffer stream instance which can be reused.
        /// </summary>
        /// <typeparam name="TPackageInfo">The type of the package info.</typeparam>
        /// <param name="receiveFilter">The receive filter.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static BufferListStream GetBufferStream<TPackageInfo>(this IReceiveFilter<TPackageInfo> receiveFilter, IList<ArraySegment<byte>> data)
            where TPackageInfo : IPackageInfo
        {
            return GetBufferStream<BufferListStream, TPackageInfo>(receiveFilter, data);
        }


        /// <summary>
        /// Gets the buffer stream instance which can be reused.
        /// </summary>
        /// <typeparam name="TStream">The type of the stream.</typeparam>
        /// <typeparam name="TPackageInfo">The type of the package info.</typeparam>
        /// <param name="receiveFilter">The receive filter.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static BufferListStream GetBufferStream<TStream, TPackageInfo>(this IReceiveFilter<TPackageInfo> receiveFilter, IList<ArraySegment<byte>> data)
            where TStream : BufferListStream, new()
            where TPackageInfo : IPackageInfo
        {
            //var stream = BufferListStream.GetCurrent<BufferListStream>(); // don't use thread context for BufferListReader for now
            var stream = new BufferListStream();
            stream.Initialize(data);
            return stream;
        }

        /// <summary>
        /// Copies data of data segment list to a data segment.
        /// </summary>
        /// <param name="packageData">The package data.</param>
        /// <param name="data">The data.</param>
        public static void CopyTo(this IList<ArraySegment<byte>> packageData, ArraySegment<byte> data)
        {
            packageData.CopyTo(data, 0, data.Count);
        }

        /// <summary>
        /// Copies data of data segment list to a data segment.
        /// </summary>
        /// <param name="packageData">The source segments data.</param>
        /// <param name="data">The destination segment.</param>
        /// <param name="srcOffset">The source offset.</param>
        /// <param name="length">The length.</param>
        public static void CopyTo(this IList<ArraySegment<byte>> packageData, ArraySegment<byte> data, int srcOffset, int length)
        {
            var innerOffset = srcOffset;
            var restLength = length;

            for(var i = 0; i < packageData.Count; i++)
            {
                var segment = packageData[i];

                if(segment.Count <= innerOffset)
                {
                    innerOffset -= segment.Count;
                    continue;
                }

                var thisLength = Math.Min(restLength, segment.Count - innerOffset);
                Array.Copy(segment.Array, innerOffset, data.Array, data.Offset, thisLength);
                restLength -= thisLength;

                if (restLength <= 0)
                    break;
            }
        }
    }
}
