namespace SuperSocket
{
    public interface IAppSession
    {
        string SessionID { get; }

        IChannel Channel { get; }

        IServerInfo Server { get; }
    }
}