using System.Threading.Tasks;

namespace SuperSocket
{
    public delegate void NewClientAcceptHandler(IListener listener, IChannel channel);

    public interface IListener
    {
        ListenOptions Options { get; }
        void Start();
        event NewClientAcceptHandler NewClientAccepted;
        Task StopAsync();
        bool IsRunning { get; }
    }
}