
using System.IO.Pipelines;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;

namespace SuperSocket.Channel
{
    public class ChannelOptions
    {
        // 1M by default
        public int MaxPackageLength { get; set; } = 1024 * 1024;
        
        // 4k by default
        public int ReceiveBufferSize { get; set; } = 1024 * 4;

        // 4k by default
        public int SendBufferSize { get; set; } = 1024 * 4;

        // trigger the read only when the stream is being consumed
        public bool ReadAsDemand { get; set; }
        
        /// <summary>
        /// in milliseconds
        /// </summary>
        /// <value></value>
        public int ReceiveTimeout { get; set; }

        /// <summary>
        /// in milliseconds
        /// </summary>
        /// <value></value>
        public int SendTimeout { get; set; }

        public ILogger Logger { get; set; }

        public Pipe In { get; set; }

        public Pipe Out { get; set; }

        public Dictionary<string, string> Values { get; set; }
    }
}
