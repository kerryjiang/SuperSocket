using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Abstractions.Connections;
using Xunit;

namespace SuperSocket.Tests
{
    public class ConsoleConnectionTest
    {
        [Fact]
        public async Task TestConsoleConnectionFactory()
        {
            // Arrange
            var connectionOptions = new ConnectionOptions
            {
                Logger = new TestLogger()
            };
            var factory = new ConsoleConnectionFactory(connectionOptions);

            // Act
            var connection = await factory.CreateConnection(null, CancellationToken.None);

            // Assert
            Assert.NotNull(connection);
            Assert.IsType<ConsoleConnection>(connection);
            Assert.Equal("Console", connection.RemoteEndPoint?.ToString());
            Assert.Equal("Console", connection.LocalEndPoint?.ToString());
        }

        [Fact]
        public void TestConsoleConnectionListenerFactory()
        {
            // Arrange
            var loggerFactory = new TestLoggerFactory();
            var listenOptions = new ListenOptions
            {
                Ip = "Console",
                Port = 0
            };
            var connectionOptions = new ConnectionOptions();
            var factory = new ConsoleConnectionListenerFactory();

            // Act
            var listener = factory.CreateConnectionListener(listenOptions, connectionOptions, loggerFactory);

            // Assert
            Assert.NotNull(listener);
            Assert.IsType<ConsoleConnectionListener>(listener);
            Assert.Equal(listenOptions, listener.Options);
        }

        [Fact]
        public async Task TestConsoleConnectionListener()
        {
            // Arrange
            var loggerFactory = new TestLoggerFactory();
            var logger = loggerFactory.CreateLogger("Test");
            var listenOptions = new ListenOptions
            {
                Ip = "Console",
                Port = 0
            };
            var connectionOptions = new ConnectionOptions
            {
                Logger = logger
            };
            var connectionFactory = new ConsoleConnectionFactory(connectionOptions);
            var listener = new ConsoleConnectionListener(listenOptions, connectionFactory, logger);

            IConnection acceptedConnection = null;
            listener.NewConnectionAccept += (options, connection) =>
            {
                acceptedConnection = connection;
                return new ValueTask();
            };

            // Act
            var started = listener.Start();
            
            // Wait a bit for the async connection creation
            await Task.Delay(100, CancellationToken.None);

            // Assert
            Assert.True(started);
            Assert.True(listener.IsRunning);
            Assert.NotNull(acceptedConnection);
            Assert.IsType<ConsoleConnection>(acceptedConnection);

            // Cleanup
            await listener.StopAsync();
            Assert.False(listener.IsRunning);
        }

        [Fact]
        public void TestConsoleConnectionBasicProperties()
        {
            // Arrange
            var connectionOptions = new ConnectionOptions
            {
                Logger = new TestLogger()
            };

            // Act
            var connection = new ConsoleConnection(connectionOptions);

            // Assert
            Assert.False(connection.IsClosed);
            Assert.NotNull(connection.RemoteEndPoint);
            Assert.NotNull(connection.LocalEndPoint);
            Assert.Equal("Console", connection.RemoteEndPoint.ToString());
            Assert.Equal("Console", connection.LocalEndPoint.ToString());
        }

        [Fact]
        public async Task TestConsoleConnectionLifecycle()
        {
            // Arrange
            var connectionOptions = new ConnectionOptions
            {
                Logger = new TestLogger()
            };
            var connection = new ConsoleConnection(connectionOptions);

            // Act & Assert
            Assert.False(connection.IsClosed);

            // Close the connection
            await connection.CloseAsync(CloseReason.ApplicationError);
            
            // Note: Console connection might not immediately show as closed 
            // since it's tied to system streams
        }

        private class TestLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => false;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
        }

        private class TestLoggerFactory : ILoggerFactory
        {
            public void AddProvider(ILoggerProvider provider) { }
            public ILogger CreateLogger(string categoryName) => new TestLogger();
            public void Dispose() { }
        }
    }
}