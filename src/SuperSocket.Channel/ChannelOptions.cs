using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO.Pipelines;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public class ChannelOptions
    {
        // 4M by default
        public int MaxPackageLength { get; set; } = 1024 * 1024 * 4;
        
        // 4k by default
        public int ReceiveBufferSize { get; set; } = 1024 * 4;


        public ILogger Logger { get; set; }

        public Pipe In { get; set; }

        public Pipe Out { get; set; }
    }
}
