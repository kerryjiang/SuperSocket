using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.Common
{
    public class SearchMarkState<T>
        where T : IEquatable<T>
    {
        public T[] Mark { get; set; }

        public int Matched { get; set; }
    }
}
