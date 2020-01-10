using SuperSocket.Channel;

namespace SuperSocket
{
    public class ServerOptions : ChannelOptions
    {
        public string Name { get; set; }

        public ListenOptions[] Listeners { get; set; }

        public int ClearIdleSessionInterval { get; set; } = 120;

        public int IdleSessionTimeOut { get; set; } = 300;
    }
}