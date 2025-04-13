using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    /// <summary>
    /// Provides extension methods for working with connections and package streams.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets an asynchronous enumerator for the package stream using the specified pipeline filter.
        /// </summary>
        /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
        /// <param name="connection">The connection to retrieve the package stream from.</param>
        /// <param name="pipelineFilter">The pipeline filter to use for processing data.</param>
        /// <returns>An asynchronous enumerator for the package stream.</returns>
        public static IAsyncEnumerator<TPackageInfo> GetPackageStream<TPackageInfo>(this IConnection connection, IPipelineFilter<TPackageInfo> pipelineFilter)
            where TPackageInfo : class
        {
            return connection.RunAsync(pipelineFilter).GetAsyncEnumerator();
        }

        /// <summary>
        /// Receives the next package from the package stream asynchronously.
        /// </summary>
        /// <typeparam name="TPackageInfo">The type of the package information.</typeparam>
        /// <param name="packageStream">The package stream to receive the package from.</param>
        /// <returns>A task that represents the asynchronous receive operation. The result contains the next package, or <c>null</c> if the stream is completed.</returns>
        public static async ValueTask<TPackageInfo> ReceiveAsync<TPackageInfo>(this IAsyncEnumerator<TPackageInfo> packageStream)
        {
            if (await packageStream.MoveNextAsync())
                return packageStream.Current;

            return default(TPackageInfo);
        }
    }
}
