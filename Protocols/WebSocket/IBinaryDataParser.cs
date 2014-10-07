using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// The basic interface for binary data parser
    /// </summary>
    public interface IBinaryDataParser
    {
        /// <summary>
        /// Parses the specified data source.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <returns></returns>
        KeyValuePair<string, object> Parse(IList<ArraySegment<byte>> dataSource);
    }
}
