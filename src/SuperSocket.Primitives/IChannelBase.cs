using System;
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IChannelBase
    {
        Task<int> SendAsync(ReadOnlyMemory<byte> data);

        event EventHandler Closed;
    }
}