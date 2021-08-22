using Microsoft.Extensions.Options;
using SuperSocket.Channel;
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace SuperSocket.Client
{
    public class GZipConnector : ConnectorBase
    {
        public GZipConnector(IConnector nextConnector)
            : base(nextConnector)
        {
        }
        public GZipConnector()
            :base()
        {
        }

        protected override async ValueTask<ConnectState> ConnectAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var stream = state.Stream;

            if (stream == null)
                throw new Exception("Stream from previous connector is null.");

            try
            {
                var gzipStream = new GZipReadWriteStream(stream, true);
                state.Stream = gzipStream;
                return state;
            }
            catch (Exception e)
            {
                return new ConnectState
                {
                    Result = false,
                    Exception = e
                };
            }
        }
    }
}