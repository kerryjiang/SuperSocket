namespace SuperSocket
{
    public interface IServerInfo
    {
        string Name { get; }

        object DataContext { get; set; }
    }
}