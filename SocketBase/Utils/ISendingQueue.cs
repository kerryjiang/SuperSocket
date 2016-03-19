using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSocket.ProtoBase;

namespace SuperSocket.SocketBase
{
    /// <summary>
    /// The inertface for sending queue
    /// </summary>
    public interface ISendingQueue : IList<ArraySegment<byte>>, IOutputBuffer
    {

    }
}
