using Microsoft.Extensions.Logging;
using SuperSocket.Channel;
using SuperSocket.Client;
using SuperSocket.ProtoBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.GZip
{
    public class GZipEasyClient<TReceivePackage> : EasyClient<TReceivePackage> where TReceivePackage : class
    {
        public GZipEasyClient(IPipelineFilter<TReceivePackage> pipelineFilter) : base(pipelineFilter)
        {
        }

        public GZipEasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, ILogger logger) : base(pipelineFilter, logger)
        {
        }

        public GZipEasyClient(IPipelineFilter<TReceivePackage> pipelineFilter, ChannelOptions options) : base(pipelineFilter, options)
        {
        }

        protected GZipEasyClient()
        {
        }
        protected override IConnector GetConnector()
        {
            var security = Security;

            if (security != null)
            {
                if (security.EnabledSslProtocols != SslProtocols.None)
                    return new SocketConnector(LocalEndPoint, new SslStreamConnector(security,new GZipConnector()));
            }

            return new SocketConnector(LocalEndPoint,new GZipConnector());
        }
    }
}
