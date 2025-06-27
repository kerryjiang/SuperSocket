using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;
using Xunit;
using System.Text;
using Xunit.Internal;

namespace SuperSocket.Tests
{
    [Trait("Category", "Connection.RunAsync")]
    public class ConnectionRunAsyncTest : TestClassBase
    {
        public ConnectionRunAsyncTest()
            : base(null)
        {
        }

        // Test connection implementation for testing purposes
        public class TestVirtualConnection : VirtualConnection
        {
            public TestVirtualConnection()
                : this(new ConnectionOptions
                {
                    Logger = NullLogger.Instance
                })
            {
            }

            public TestVirtualConnection(ConnectionOptions options)
                : base(options)
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 12345);
                LocalEndPoint = new IPEndPoint(IPAddress.Loopback, 54321);
            }

            protected override void Close()
            {
            }

            protected override async ValueTask<int> FillInputPipeWithDataAsync(Memory<byte> memory, CancellationToken cancellationToken)
            {
                var tcs = new TaskCompletionSource<int>();
                cancellationToken.Register(() => tcs.TrySetResult(0));
                return await tcs.Task.ConfigureAwait(false);
            }

            protected override ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
            {
                // For testing, we just return the length as "sent"
                return new ValueTask<int>((int)buffer.Length);
            }

            public async Task CompleteInput()
            {
                await Input.Writer.FlushAsync();
                await Input.Writer.CompleteAsync();
            }
        }

        [Fact]
        public async Task RunAsync_Should_Process_Packages_Successfully()
        {
            // Arrange
            var testData = Encoding.UTF8.GetBytes("Hello\r\nWorld\r\nTest\r\n");

            using var connection = new TestVirtualConnection();
            var filter = new CommandLinePipelineFilter();
            filter.Decoder = new DefaultStringPackageDecoder();
            var packages = new List<StringPackageInfo>();

            // Act
            await connection.WriteInputPipeDataAsync(testData, CancellationToken);
            await connection.CompleteInput();

            await foreach (var package in connection.RunAsync(filter))
            {
                packages.Add(package);
            }

            // Assert
            Assert.Equal(3, packages.Count);
            Assert.Equal("Hello", packages[0].Key);
            Assert.Equal("World", packages[1].Key);
            Assert.Equal("Test", packages[2].Key);
        }

        [Fact]
        public async Task RunAsync_Should_Handle_Partial_Data()
        {
            // Arrange
            using var connection = new TestVirtualConnection();
            var filter = new CommandLinePipelineFilter();
            filter.Decoder = new DefaultStringPackageDecoder();
            var packages = new List<StringPackageInfo>();

            // Act - Write partial data first
            await connection.WriteInputPipeDataAsync(Encoding.UTF8.GetBytes("Hello"), CancellationToken);
            
            // Use cancellation token to avoid infinite wait
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            
            var runTask = Task.Run(async () =>
            {
                await foreach (var package in connection.RunAsync(filter).WithCancellation(cts.Token))
                {
                    packages.Add(package);
                }
            }, CancellationToken);

            // Wait a bit then complete the message
            await Task.Delay(10, CancellationToken);

            Assert.Empty(packages); // Should not have received any complete packages yet
            await connection.WriteInputPipeDataAsync(Encoding.UTF8.GetBytes("\r\n"), CancellationToken);
            await connection.CompleteInput();

            // Wait for the task to complete or timeout
            try
            {
                await runTask;
            }
            catch (OperationCanceledException)
            {
                // Expected if no complete packages are received within timeout
            }

            // Assert - Should have received the complete package
            Assert.Single(packages);
            Assert.Equal("Hello", packages[0].Key);
        }

        [Fact]
        public async Task RunAsync_Should_Handle_Multiple_Packages_In_Single_Buffer()
        {
            // Arrange
            var testData = Encoding.UTF8.GetBytes("Package1\r\nPackage2\r\nPackage3\r\n");

            using var connection = new TestVirtualConnection();
            var filter = new CommandLinePipelineFilter();
            filter.Decoder = new DefaultStringPackageDecoder();
            var packages = new List<StringPackageInfo>();

            // Act
            await connection.WriteInputPipeDataAsync(testData, CancellationToken);
            await connection.CompleteInput();

            await foreach (var package in connection.RunAsync(filter))
            {
                packages.Add(package);
            }

            // Assert
            Assert.Equal(3, packages.Count);
            Assert.Equal("Package1", packages[0].Key);
            Assert.Equal("Package2", packages[1].Key);
            Assert.Equal("Package3", packages[2].Key);
        }

        [Fact]
        public async Task RunAsync_Should_Handle_Empty_Data()
        {
            // Arrange
            using var connection = new TestVirtualConnection();
            var filter = new CommandLinePipelineFilter();
            filter.Decoder = new DefaultStringPackageDecoder();
            var packages = new List<StringPackageInfo>();

            // Act
            await connection.CompleteInput(); // Complete immediately with no data

            await foreach (var package in connection.RunAsync(filter))
            {
                packages.Add(package);
            }

            // Assert
            Assert.Empty(packages);
        }

        [Fact]
        public async Task RunAsync_Should_Handle_Connection_Cancellation()
        {
            // Arrange
            using var connection = new TestVirtualConnection();
            var filter = new CommandLinePipelineFilter();
            filter.Decoder = new DefaultStringPackageDecoder();
            var packages = new List<StringPackageInfo>();

            // Act
            var runTask = Task.Run(async () =>
            {
                await foreach (var package in connection.RunAsync(filter))
                {
                    packages.Add(package);
                }
            }, CancellationToken);

            // Close the connection after a short delay
            await Task.Delay(10, CancellationToken);
            await connection.CloseAsync(CloseReason.LocalClosing);

            // Wait for the task to complete
            await runTask;

            // Assert
            Assert.Empty(packages);
            Assert.True(connection.IsClosed);
        }

        // Pipeline filter that throws exceptions for testing error handling
        public class ExceptionThrowingPipelineFilter : IPipelineFilter<StringPackageInfo>
        {
            public IPackageDecoder<StringPackageInfo> Decoder { get; set; }
            public IPipelineFilter<StringPackageInfo> NextFilter => null;
            public object Context { get; set; }

            public StringPackageInfo Filter(ref SequenceReader<byte> reader)
            {
                throw new InvalidOperationException("Test filter exception");
            }

            public void Reset()
            {
            }
        }

        [Fact]
        public async Task RunAsync_Should_Handle_Filter_Exceptions()
        {
            // Arrange
            var testData = Encoding.UTF8.GetBytes("Test\r\n");

            using var connection = new TestVirtualConnection();
            var packages = new List<StringPackageInfo>();
            var filter = new ExceptionThrowingPipelineFilter();
            filter.Decoder = new DefaultStringPackageDecoder();

            // Act & Assert
            await connection.WriteInputPipeDataAsync(testData, CancellationToken);
            await connection.CompleteInput();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await foreach (var package in connection.RunAsync(filter))
                {
                    // Should not reach here
                }
            });

            Assert.Equal("Test filter exception", exception.Message);
        }
    }
}
