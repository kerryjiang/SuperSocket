using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public interface IPipelineProcessor
    {
        ProcessResult Process(ArraySegment<byte> segment, object state);

        ReceiveCache Cache { get; }
    }
}
