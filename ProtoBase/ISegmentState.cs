using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.ProtoBase
{
    public interface ISegmentState
    {
        void IncreaseReference();

        int DecreaseReference();
    }
}
