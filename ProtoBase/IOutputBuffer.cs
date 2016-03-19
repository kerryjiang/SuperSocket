using System;
using System.Collections.Generic;

namespace SuperSocket.ProtoBase
{

    /// <summary>
    /// The output buffer interface
    /// </summary>
    public interface IOutputBuffer
    {
        /// <summary>
        /// add single one item into the queue
        /// </summary>
        /// <param name="item">the item to be inserted</param>
        void Add(ArraySegment<byte> item);

        /// <summary>
        /// add multiple items into the queue
        /// </summary>
        /// <param name="itemsSource">the source cof the multiple items to be inserted</param>
        void AddRange(Func<IList<ArraySegment<byte>>> itemsSource);
    }
}
