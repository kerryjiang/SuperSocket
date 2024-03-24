using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Connection
{
    public interface IPipeConnection
    {
        IPipelineFilter PipelineFilter { get; }

        PipeReader InputReader { get; }

        PipeWriter OutputWriter { get; }
    }
}
