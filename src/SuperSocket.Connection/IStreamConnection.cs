using System.IO;

namespace SuperSocket.Connection
{
    internal interface IStreamConnection
    {
        Stream Stream { get; }
    }
}