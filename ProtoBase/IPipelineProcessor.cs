using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public interface IPipelineProcessor
    {
        ProcessState Process(ArraySegment<byte> rawData);

        ReceiveCache Cache { get; }
    }
}
