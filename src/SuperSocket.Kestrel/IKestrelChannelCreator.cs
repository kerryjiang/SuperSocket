using Microsoft.AspNetCore.Connections;
using SuperSocket.Channel;
using System;
using System.Threading.Tasks;

namespace SuperSocket.Kestrel;

public interface IKestrelChannelCreator : IChannelCreator
{
    event Func<ConnectionContext, ValueTask<IChannel>> ChannelFactoryAsync;
}