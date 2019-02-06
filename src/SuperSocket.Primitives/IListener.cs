using System;
using System.Threading.Tasks;

namespace SuperSocket
{
    public delegate void NewClientAcceptHandler(IListener listener, IChannel channel);

    public interface IListener
    {
        ListenOptions Options { get; }
        bool Start();
        event NewClientAcceptHandler NewClientAccepted;
        Task StopAsync();
    }
}