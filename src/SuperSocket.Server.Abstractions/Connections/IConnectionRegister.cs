
using System;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;
using SuperSocket.Connection;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions.Connections
{
    public interface IConnectionRegister
    {
        Task RegisterConnection(object connection);
    }
}