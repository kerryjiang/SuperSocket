using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    /// <summary>
    /// SearchMarkState
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SearchMarkState<T>
        where T : IEquatable<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchMarkState&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="mark">The mark.</param>
        public SearchMarkState(T[] mark)
        {
            Mark = mark;
        }

        /// <summary>
        /// Gets the mark.
        /// </summary>
        public T[] Mark { get; private set; }

        /// <summary>
        /// Gets or sets whether matched already.
        /// </summary>
        /// <value>
        /// The matched.
        /// </value>
        public int Matched { get; set; }


        /// <summary>
        /// Changes the search mark.
        /// </summary>
        /// <param name="mark">The mark.</param>
        public void Change(T[] mark)
        {
            Mark = mark;
            Matched = 0;
        }
    }
}
