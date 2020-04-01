using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Channel;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace SuperSocket.Client.Proxy
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc1928
    /// https://en.wikipedia.org/wiki/SOCKS
    /// </summary>
    public class Socks5Connector : ProxyConnectorBase
    {
        private string _username;
        private string _password;

        public Socks5Connector(EndPoint proxyEndPoint)
            : base(proxyEndPoint)
        {

        }

        public Socks5Connector(EndPoint proxyEndPoint, string username, string password)
            : this(proxyEndPoint)
        {
            _username = username;
            _password = password;
        }

        protected override async ValueTask<ConnectState> ConnectProxyAsync(EndPoint remoteEndPoint, ConnectState state, CancellationToken cancellationToken)
        {
            var channel = state.CreateChannel<Socks5Pack>(new Socks5AuthPipelineFilter(), new ChannelOptions());
            var packStream = channel.GetPackageStream();

            // send auth request 0
            // await channel.SendAsync(...);            
            var pack = await packStream.ReceiveAsync();
            // handle response


            // send auth request 1
            // await channel.SendAsync(...);
            pack = await packStream.ReceiveAsync();
            // handle response

            // send address request
            // await channel.SendAsync(...);            
            pack = await packStream.ReceiveAsync();
            // handle response

            await channel.DetachAsync();            
            return state;
        }

        public class Socks5Address
        {
            public IPAddress IPAddress { get; set; }
            public string DomainName { get; set; }
        }

        public class Socks5Pack
        {
            public byte Version { get; set; }
            public byte Status { get; set; }
            public byte Reserve { get; set; }
            public Socks5Address DestAddr { get; set; }
            public short DestPort { get; set; }
        }

        public class Socks5AuthPipelineFilter : FixedSizePipelineFilter<Socks5Pack>
        {
            public int AuthStep { get; set; }

            public Socks5AuthPipelineFilter()
                : base(2)
            {

            }

            protected override Socks5Pack DecodePackage(ReadOnlySequence<byte> buffer)
            {
                var reader = new SequenceReader<byte>(buffer);
                reader.TryRead(out byte version);
                reader.TryRead(out byte status);

                if (AuthStep == 0)
                    NextFilter = new Socks5AuthPipelineFilter { AuthStep = 1 };
                else
                    NextFilter = new Socks5AddressPipelineFilter();

                return new Socks5Pack
                {
                    Version = version,
                    Status = status
                };
            }
        }

        public class Socks5AddressPipelineFilter : FixedHeaderPipelineFilter<Socks5Pack>
        {
            public Socks5AddressPipelineFilter()
                : base(5)
            {

            }

            protected override int GetBodyLengthFromHeader(ReadOnlySequence<byte> buffer)
            {
                var reader = new SequenceReader<byte>(buffer);
                reader.Advance(3);
                reader.TryRead(out byte addressType);

                if (addressType == 0x01)
                    return 6 - 1;
                
                if (addressType == 0x04)
                    return 18 - 1;

                if (addressType == 0x03)
                {
                    reader.TryRead(out byte domainLen);
                    return domainLen + 2;
                }

                throw new Exception($"Unsupported addressType: {addressType}");
            }

            protected override Socks5Pack DecodePackage(ReadOnlySequence<byte> buffer)
            {
                var reader = new SequenceReader<byte>(buffer);
                reader.TryRead(out byte version);
                reader.TryRead(out byte status);
                reader.TryRead(out byte reserve);

                reader.TryRead(out byte addressType);

                var address = new Socks5Address();                

                if (addressType == 0x01)
                {
                    var addrLen = 4;
                    address.IPAddress = new IPAddress(reader.Sequence.Slice(reader.Consumed, addrLen).ToArray());
                    reader.Advance(addrLen);
                }
                else if (addressType == 0x04)
                {
                    var addrLen = 16;
                    address.IPAddress = new IPAddress(reader.Sequence.Slice(reader.Consumed, addrLen).ToArray());
                    reader.Advance(addrLen);
                }
                else if (addressType == 0x03)
                {
                    reader.TryRead(out byte addrLen);
                    var seq = reader.Sequence.Slice(reader.Consumed, addrLen);
                    address.DomainName = seq.GetString(Encoding.ASCII);
                    reader.Advance(addrLen);
                }
                else
                {
                    throw new Exception($"Unsupported addressType: {addressType}");
                }

                reader.TryReadBigEndian(out short port);

                return new Socks5Pack
                {
                    Version = version,
                    Status = status,
                    Reserve = reserve,
                    DestAddr = address,
                    DestPort = port
                };
            }
        }
    }
}