using Microsoft.Extensions.DependencyInjection;
using SuperSocket.Command;
using SuperSocket.ProtoBase;
using SuperSocket.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SuperSocket.Tests
{
    [Trait("Category", "PackageHandlingContextAccessor")]
    public class PackageHandlingContextAccessorTest : TestClassBase
    {
        public PackageHandlingContextAccessorTest(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public async Task TestUsePackageHandlingContextAccessor()
        {

            ISuperSocketHostBuilder<StringPackageInfo> superSocketHostBuilder = CreateSocketServerBuilder<StringPackageInfo, CommandLinePipelineFilter>()
                            .UseCommand(commandOptions =>
                            {
                                commandOptions.AddCommand<TestCommand>();
                            })
                            .ConfigureServices((htx, services) =>
                            {
                                services.AddSingleton<ITestOutputHelper>(OutputHelper);
                            })
                            .UsePackageHandlingContextAccessor();
            using (var server = superSocketHostBuilder.BuildAsServer())
            {
                Assert.Equal("TestServer", server.Name);

                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");


                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await client.ConnectAsync(GetDefaultServerEndPoint());
                OutputHelper.WriteLine("Connected.");

                await Task.Delay(1000);

                using (var stream = new NetworkStream(client))
                using (var streamReader = new StreamReader(stream, Utf8Encoding, true))
                using (var streamWriter = new StreamWriter(stream, Utf8Encoding, 1024 * 1024 * 4))
                {
                    await streamWriter.WriteAsync("TestCommand Package 1\r\n");
                    await streamWriter.FlushAsync();
                    OutputHelper.WriteLine("send 'Package 1'.");
                    var line = await streamReader.ReadLineAsync();
                    Assert.Equal("Package 1", line);

                    await streamWriter.WriteAsync("TestCommand Package 2\r\n");
                    await streamWriter.FlushAsync();
                    OutputHelper.WriteLine("send 'Package 2'.");
                    line = await streamReader.ReadLineAsync();
                    Assert.Equal("Package 2", line);

                }

                await server.StopAsync();
            }
        }

        public class TestCommand : IAsyncCommand<StringPackageInfo>
        {
            private readonly IPackageHandlingContextAccessor<StringPackageInfo> _packageHandlingContextAccessor;

            public TestCommand(ITestOutputHelper outputHelper, IPackageHandlingContextAccessor<StringPackageInfo> packageHandlingContextAccessor)
            {
                OutputHelper = outputHelper;
                _packageHandlingContextAccessor = packageHandlingContextAccessor;
            }
            private readonly Random _random = new Random();

            public ITestOutputHelper OutputHelper { get; }

            public async ValueTask ExecuteAsync(IAppSession session, StringPackageInfo package)
            {
                Assert.NotNull(_packageHandlingContextAccessor.PackageHandlingContext.AppSession);
                Assert.NotNull(_packageHandlingContextAccessor.PackageHandlingContext.PackageInfo);
                OutputHelper.WriteLine($"package.Body: {package.Body} || packageinfo from packageHandlingContextAccessor: {_packageHandlingContextAccessor.PackageHandlingContext.PackageInfo.Body}");
                Assert.Equal(_packageHandlingContextAccessor.PackageHandlingContext.PackageInfo.Body, package.Body);

                Assert.Equal(_packageHandlingContextAccessor.PackageHandlingContext.PackageInfo.Parameters[1], package.Parameters[1]);
                OutputHelper.WriteLine($"Before reassigning Parameters[1]    Parameters[1]: {package.Parameters[1]} || Parameters[1] from packageHandlingContextAccessor: {_packageHandlingContextAccessor.PackageHandlingContext.PackageInfo.Parameters[1]}");
                _packageHandlingContextAccessor.PackageHandlingContext.PackageInfo.Parameters[1] += "%";
                OutputHelper.WriteLine($"After reassigning Parameters[1]    Parameters[1]: {package.Parameters[1]} || Parameters[1] from packageHandlingContextAccessor: {_packageHandlingContextAccessor.PackageHandlingContext.PackageInfo.Parameters[1]}");
                Assert.Equal(_packageHandlingContextAccessor.PackageHandlingContext.PackageInfo.Parameters[1], package.Parameters[1]);
                await session.SendAsync(Encoding.UTF8.GetBytes(package.Body + "\r\n"));
            }
        }
    }


}
