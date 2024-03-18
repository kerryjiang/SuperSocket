using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    public interface IPipeConnection
    {
        PipeReader InputReader { get; }

        PipeWriter OutputWriter { get; }

        IPipelineFilter PipelineFilter { get; }
    }
}
