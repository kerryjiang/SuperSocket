namespace SuperSocket
{
    public class ServerOptions
    {
        public string Name { get; set; }

        public ListenOptions[] Listeners { get; set; }
    }
}