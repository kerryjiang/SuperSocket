using System;
using System.Threading.Tasks;

namespace SuperSocket
{
    public delegate void NewClientAcceptHandler(IListener listener, IChannelBase channel, object state);

    public interface IListener
    {
        ListenOptions Options { get; }
        bool Start();
        event NewClientAcceptHandler NewClientAccepted;
        Task StopAsync();
    }
}