using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SuperSocket.Connection;
using SuperSocket.ProtoBase;
using Xunit;

namespace SuperSocket.Tests;

public class PipeConnectionBaseTest
{
    [Fact]
    public async Task TestDetachPreservesBufferedDataAfterCurrentPackage()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connection = new DetachablePipeConnection();
        var receivedPackages = new List<string>();
        var firstPackageReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var continuePackageLoop = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var readTask = Task.Run(async () =>
        {
            await foreach (var package in connection.RunAsync(new LinePipelineFilter()))
            {
                receivedPackages.Add(package.Text);

                if (receivedPackages.Count == 1)
                {
                    firstPackageReceived.TrySetResult();
                    await continuePackageLoop.Task;
                }
            }
        }, cancellationToken);

        await connection.InputWriter.WriteAsync(Encoding.UTF8.GetBytes("READY\r\nRAW\r\n"), cancellationToken);
        await firstPackageReceived.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        var detachTask = connection.DetachAsync().AsTask();
        continuePackageLoop.SetResult();

        await detachTask.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        await readTask.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        Assert.Collection(receivedPackages, package => Assert.Equal("READY", package));

        var result = await connection.RawReader.ReadAsync(cancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        Assert.Equal("RAW\r\n", Encoding.UTF8.GetString(result.Buffer.ToArray()));
        connection.RawReader.AdvanceTo(result.Buffer.End);
    }

    private sealed class DetachablePipeConnection : PipeConnectionBase
    {
        private readonly Pipe _input;

        public DetachablePipeConnection()
            : this(new Pipe(), new Pipe())
        {
        }

        private DetachablePipeConnection(Pipe input, Pipe output)
            : base(input.Reader, output.Writer, new ConnectionOptions { Logger = NullLogger.Instance })
        {
            _input = input;
        }

        public PipeWriter InputWriter => _input.Writer;

        public PipeReader RawReader => _input.Reader;

        protected override void Close()
        {
        }

        protected override ValueTask CompleteReaderAsync(PipeReader reader, bool isDetaching)
        {
            return ValueTask.CompletedTask;
        }

        protected override ValueTask CompleteWriterAsync(PipeWriter writer, bool isDetaching)
        {
            return ValueTask.CompletedTask;
        }
    }
}
