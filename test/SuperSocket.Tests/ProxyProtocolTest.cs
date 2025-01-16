using System;
using System.Buffers;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Host;
using SuperSocket.Server.Abstractions;
using Xunit;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using SuperSocket.Server.Abstractions.Session;

namespace SuperSocket.Tests
{
    [Trait("Category", "ProxyProtocol")]
    public class ProxyProtocolTest : FixedHeaderProtocolTest
    {
        private static readonly IPAddress[] _ipv4AddressPool = (new []
            {
                "247.47.227.3",
                "112.207.192.91",
                "123.193.169.24",
                "191.213.152.251",
                "7.132.159.148",
                "214.75.171.159",
                "170.103.166.188",
                "228.111.89.87",
                "4.122.43.89",
                "206.222.157.16"
            }).Select(ip => IPAddress.Parse(ip)).ToArray();

        private static readonly IPAddress[] _ipv6AddressPool = (new[]
            {
                "4466:a5cd:cacc:faa1:4522:055e:9094:f1a3",
                "f438:4e9c:0d38:6ae5:ef44:4b0e:c160:a254",
                "529f:bc3e:2e56:8365:dc06:5772:87a5:e658",
                "4b19:5c07:d470:018a:36a0:b31a:59aa:cd48",
                "2a61:61ff:8a01:5473:0091:e416:aeda:f924",
                "0f29:faaa:c984:c0fd:d0d2:36ca:7132:933e",
                "1598:7240:de55:0803:305a:b7f4:4eab:fd00",
                "6431:494f:9a92:4ea7:5645:a3ab:945a:ca72",
                "48d4:c5d6:b3e8:9859:dc0f:a8d0:e085:3518",
                "8e72:3b78:2b3e:33ad:7b12:4b14:37de:e9f1"
            }).Select(ip => IPAddress.Parse(ip)).ToArray();

        private static readonly  Random _rd = new Random();

        public ProxyProtocolTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        protected override IHostConfigurator CreateHostConfigurator(Type hostConfiguratorType)
        {
            var radomNumber = _rd.Next(0, 1000);
            var addressPool = radomNumber % 2 == 0 ? _ipv4AddressPool : _ipv6AddressPool;

            var sourceIPEndPoint = new IPEndPoint(addressPool[_rd.Next(0, addressPool.Length)], _rd.Next(100, 9999));
            var destinationIPEndPoint = new IPEndPoint(addressPool[_rd.Next(0, addressPool.Length)], _rd.Next(100, 9999));

            return new ProxyProtocolV2HostConfigurator(base.CreateHostConfigurator(hostConfiguratorType), sourceIPEndPoint, destinationIPEndPoint);
        }

        protected override Dictionary<string, string> LoadMemoryConfig(Dictionary<string, string> configSettings)
        {
            base.LoadMemoryConfig(configSettings);
            configSettings["serverOptions:enableProxyProtocol"] = "true";
            return configSettings;
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator), 1, "123.193.169.24", 5444, "228.111.89.87", 199)]
        [InlineData(typeof(RegularHostConfigurator), 1, "529f:bc3e:2e56:8365:dc06:5772:87a5:e658", 5444, "48d4:c5d6:b3e8:9859:dc0f:a8d0:e085:3518", 199)]
        [InlineData(typeof(RegularHostConfigurator), 2, "123.193.169.24", 5444, "228.111.89.87", 199)]
        [InlineData(typeof(RegularHostConfigurator), 2, "529f:bc3e:2e56:8365:dc06:5772:87a5:e658", 5444, "48d4:c5d6:b3e8:9859:dc0f:a8d0:e085:3518", 199)]
        public async Task TestProxyEndPoints(Type hostConfiguratorType, int proxyProtocolVersion, string sourceIP, int sourcePort, string destinationIP, int destinationPort)
        {
            var hostConfigurator = base.CreateHostConfigurator(hostConfiguratorType);

            var sourceIPAddress = IPAddress.Parse(sourceIP);
            var destinationIPAddress = IPAddress.Parse(destinationIP);

            hostConfigurator = proxyProtocolVersion == 1
                ? new ProxyProtocolV1HostConfigurator(hostConfigurator, new IPEndPoint(sourceIPAddress, sourcePort), new IPEndPoint(destinationIPAddress, destinationPort))
                : new ProxyProtocolV2HostConfigurator(hostConfigurator, new IPEndPoint(sourceIPAddress, sourcePort), new IPEndPoint(destinationIPAddress, destinationPort));

            var taskCompletionSource = new TaskCompletionSource<IAppSession>();

            using (var server = CreateSocketServerBuilder<TextPackageInfo, MyFixedHeaderPipelineFilter>(hostConfigurator)
                .UsePackageHandler(async (s, p) =>
                {
                    taskCompletionSource.SetResult(s);
                    await Task.CompletedTask;
                })
                .ConfigureAppConfiguration((HostBuilder, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(LoadMemoryConfig(new Dictionary<string, string>()));
                }).BuildAsServer() as IServer)
            {
                await server.StartAsync();

                using (var socket = CreateClient(hostConfigurator))
                {
                    using (var socketStream = await hostConfigurator.GetClientStream(socket))
                    using (var reader = hostConfigurator.GetStreamReader(socketStream, Utf8Encoding))
                    using (var writer = new ConsoleWriter(socketStream, Utf8Encoding, 1024 * 8))
                    {
                        var line = Guid.NewGuid().ToString();
                        writer.Write(CreateRequest(line));
                        writer.Flush();

                        var session = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromMinutes(1));

                        Assert.NotNull(session.Connection.ProxyInfo);

                        Assert.Equal(sourceIPAddress, session.Connection.ProxyInfo.SourceIPAddress);
                        Assert.Equal(sourcePort, session.Connection.ProxyInfo.SourcePort);

                        Assert.Equal(destinationIPAddress, session.Connection.ProxyInfo.DestinationIPAddress);
                        Assert.Equal(destinationPort, session.Connection.ProxyInfo.DestinationPort);                      
                    }
                }

                await server.StopAsync();
            }
        }
    }
}
