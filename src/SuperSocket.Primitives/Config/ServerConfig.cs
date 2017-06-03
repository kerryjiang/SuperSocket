namespace SuperSocket.Config
{
    public class ServerConfig
    {
        public string Name { get; set; }

        public ListenerConfig[] Listeners { get; set; }
    }
}