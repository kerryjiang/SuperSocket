using System.Threading.Tasks;

namespace SuperSocket.Connection
{
    public interface IConnectionFactory
    {
        Task<IConnection> CreateConnection(object connection);
    }
}