using SuperSocket.Channel;

namespace SuperSocket
{
    public class ServerOptions : ChannelOptions
    {
        public string Name { get; set; }

        public ListenOptions[] Listeners { get; set; }
    }
}