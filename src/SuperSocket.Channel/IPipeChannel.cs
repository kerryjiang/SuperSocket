using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public interface IPipeChannel
    {
        Pipe In { get; }

        Pipe Out { get; }

        IPipelineFilter PipelineFilter { get; }
    }
}
