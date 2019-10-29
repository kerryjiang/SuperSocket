
using System;
using Microsoft.Extensions.Logging;
using SuperSocket.ProtoBase;
using SuperSocket.Channel;
using System.Threading.Tasks;

namespace SuperSocket
{
    public interface IChannelRegister
    {
        Task RegisterChannel(object connection);
    }
}