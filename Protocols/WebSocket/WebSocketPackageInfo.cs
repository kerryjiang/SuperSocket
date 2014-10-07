using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSocket.WebSocket
{
    /// <summary>
    /// WebSocket protocol package instance
    /// </summary>
    public class WebSocketPackageInfo : StringPackageInfo
    {
        /// <summary>
        /// Gets the binary data.
        /// </summary>
        /// <value>
        /// The binary data.
        /// </value>
        public IList<ArraySegment<byte>> BinaryData { get; private set; }

        /// <summary>
        /// Gets the request object.
        /// </summary>
        /// <value>
        /// The request object.
        /// </value>
        public object Object { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketPackageInfo"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceParser">The source parser.</param>
        public WebSocketPackageInfo(string source, IStringParser sourceParser)
            : base(source, sourceParser)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketPackageInfo"/> class.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="binaryDataParser">The binary data parser.</param>
        /// <param name="stringParser">The string parser.</param>
        public WebSocketPackageInfo(IList<ArraySegment<byte>> dataSource, IBinaryDataParser binaryDataParser, IStringParser stringParser)
        {
            if (binaryDataParser == null)
            {
                BinaryData = dataSource;
                return;
            }

            var dataPair = binaryDataParser.Parse(dataSource);

            Key = dataPair.Key;

            // plain text package
            if (dataPair.Value.GetType() == typeof(string) && stringParser != null)
            {
                InitializeData((string)dataPair.Value, stringParser);
                return;
            }

            Object = dataPair.Value;            
        }
    }
}
