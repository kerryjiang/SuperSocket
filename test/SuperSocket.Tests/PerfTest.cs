using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using SuperSocket.Server.Host;
using Xunit;

namespace SuperSocket.Tests
{
    public class PerfTest : TestClassBase
    {
        public PerfTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        [InlineData(typeof(KestralConnectionHostConfigurator))]
        public async Task ConcurrentSendReceive(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);
            var connectionCount = 10;
            var runTime = TimeSpan.FromMinutes(1);

            using (var server = CreateSocketServerBuilder<TextPackageInfo, LinePipelineFilter>(hostConfigurator)
                .UsePackageHandler(async (s, p) =>
                {
                    await s.SendAsync(Utf8Encoding.GetBytes(p.Text + "\r\n"));
                }).BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync(TestContext.Current.CancellationToken));
                OutputHelper.WriteLine("Started.");

                using var cancellationTokenSource = new CancellationTokenSource();

                var tasks = Enumerable
                    .Range(0, connectionCount)
                    .Select(_ => RunConnectionAsync(hostConfigurator, cancellationTokenSource.Token))
                    .ToArray();

                cancellationTokenSource.CancelAfter(runTime);

                await Task.Delay(runTime, CancellationToken.None);

                var rounds = await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(10), CancellationToken.None);

                OutputHelper.WriteLine($"Total rounds: {rounds.Sum()}.");
            }
        }

        private async Task<int> RunConnectionAsync(IHostConfigurator hostConfigurator, CancellationToken cancellationToken)
        {
            var round = 0;

            using (var socket = await hostConfigurator.CreateConnectedClientAsync())
            using (var clientStream = await hostConfigurator.GetClientStream(socket))
            using (var streamReader = new StreamReader(clientStream, Utf8Encoding, true))
            using (var streamWriter = new StreamWriter(clientStream, Utf8Encoding, 1024 * 1024 * 4))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var text = Guid.NewGuid().ToString();

                    await streamWriter.WriteAsync($"{text}\r\n".AsMemory());
                    await streamWriter.FlushAsync();

                    var line = await streamReader.ReadLineAsync();
                    Assert.Equal(text, line);

                    round++;
                }
            }

            return round;
        }
    }
}