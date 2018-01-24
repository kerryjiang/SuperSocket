namespace Tests
{
    public abstract class TestBase
    {
        protected abstract void RegisterServices(IServiceCollection services);

        protected SocketServer CreateSocketServer<TPackageInfo, TPipelineFilter>(Dictionary<string, string> configDict = null, Action<IAppSession, TPackageInfo> packageHandler = null)
            where TPackageInfo : class
            where TPipelineFilter : IPipelineFilter<TPackageInfo>, new()
        {
            if (configDict == null)
            {
                configDict = new Dictionary<string, string>
                {
                    { "name", "TestServer" },
                    { "listeners:0:ip", "Any" },
                    { "listeners:0:port", "4040" }
                };
            }

            var server = new SocketServer();

            var services = new ServiceCollection();

            var builder = new ConfigurationBuilder().AddInMemoryCollection(configDict);
            var config = builder.Build();

            RegisterServices(services);

            Assert.True(server.Configure<TPackageInfo, TPipelineFilter>(config, services, packageHandler));

            return server;
        }

        [Fact]
        public async Task TestSessionCount()
        {
            var server = CreateSocketServer<FakePackageInfo, FakePipelineFilter>();

            Assert.Equal("TestServer", server.Name);

            Assert.True(server.Start());
            Assert.Equal(0, server.SessionCount);

            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));

            await Task.Delay(1);

            Assert.Equal(1, server.SessionCount);

            server.Stop();
        }

        [Fact]
        public async Task TestConsoleProtocol()
        {
            var server = CreateSocketServer<LinePackageInfo, NewLinePipelineFilter>();

            Assert.True(server.Start());
            Assert.Equal(0, server.SessionCount);

            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040));

            using (var stream = new NetworkStream(client))
            using (var streamReader = new StreamReader(stream, Encoding.UTF8, true))
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024 * 1024 * 4))
            {
                await streamWriter.WriteLineAsync("Hello World");
                var line = await streamReader.ReadLineAsync();
                Assert.Equal("Hello World", line);
            }

            server.Stop();
        }
    }
}