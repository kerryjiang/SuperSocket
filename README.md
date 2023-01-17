# SuperSocket

[![Join the chat at https://gitter.im/supersocket/community](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/supersocket/community)
[![Build](https://github.com/kerryjiang/SuperSocket/workflows/build/badge.svg)](https://github.com/kerryjiang/SuperSocket/actions?query=workflow%3Abuild)
[![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.svg?style=flat)](https://www.nuget.org/packages/SuperSocket/)
[![NuGet](https://img.shields.io/nuget/dt/SuperSocket.svg)](https://www.nuget.org/packages/SuperSocket)
[![Badge](https://img.shields.io/badge/link-996.icu-red.svg)](https://996.icu/#/en_US)


**SuperSocket** is a light weight extensible socket application framework. You can use it to build an always connected socket application easily without thinking about how to use socket, how to maintain the socket connections and how socket works. It is a pure C# project which is designed to be extended, so it is easy to be integrated to your existing systems as long as they are developed in .NET language.


- **Project homepage**:		[https://www.supersocket.net/](https://www.supersocket.net/)
- **Documentation**:		[https://docs.supersocket.net/](https://docs.supersocket.net/)
- **License**: 				[https://www.apache.org/licenses/LICENSE-2.0](https://www.apache.org/licenses/LICENSE-2.0)

---

##### Nuget Packages

| Package | MyGet Version | NuGet Version | Download |
| :------|:------------:|:------------:|:--------:|
| **SuperSocket**  <br /> (all in one) | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.svg?style=flat)](https://www.nuget.org/packages/SuperSocket/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.svg?style=flat)](https://www.nuget.org/packages/SuperSocket/) |
| **SuperSocket.WebSocketServer** <br /> (all in one for WebSocket server) | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.WebSocketServer)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.WebSocketServer) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.WebSocketServer.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.WebSocketServer/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.WebSocketServer.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.WebSocketServer/) |
| **SuperSocket.ProtoBase** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.ProtoBase)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.ProtoBase) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.ProtoBase.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.ProtoBase/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.ProtoBase.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.ProtoBase/) |
| **SuperSocket.Primitives** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Primitives)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Primitives) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Primitives.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Primitives/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.Primitives.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Primitives/) |
| **SuperSocket.Channel** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Channel)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Channel) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Channel.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Channel/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.Channel.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Channel/) |
| **SuperSocket.Server** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Server)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Server) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Server.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Server/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.Server.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Server/) |
| **SuperSocket.Command** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Command)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Command) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Command.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Command/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.Command.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Command/) |
| **SuperSocket.SessionContainer** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.SessionContainer)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.SessionContainer) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.SessionContainer.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.SessionContainer/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.SessionContainer.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.SessionContainer/) |
| **SuperSocket.Client** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Client)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Client) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Client.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Client/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.Client.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Client/) |
| **SuperSocket.Client.Proxy** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Client.Proxy)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Client.Proxy) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Client.Proxy.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Client.Proxy/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.Client.Proxy.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Client.Proxy/) |
| **SuperSocket.WebSocket** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.WebSocket)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.WebSocket) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.WebSocket.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.WebSocket/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.WebSocket.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.WebSocket/) |
| **SuperSocket.WebSocket.Server** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.WebSocket.Server)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.WebSocket.Server) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.WebSocket.Server.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.WebSocket.Server/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.WebSocket.Server.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.WebSocket.Server/) |
| **SuperSocket.Udp** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Udp)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Udp) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Udp.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Udp/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.Udp.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Udp/) |
| **SuperSocket.GZip** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.GZip)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.GZip) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.GZip.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.GZip/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.GZip.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.GZip/) |
| **SuperSocket.SerialIO** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.SerialIO)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.SerialIO) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.SerialIO.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.SerialIO/)| [![NuGet Download](https://img.shields.io/nuget/dt/SuperSocket.SerialIO.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.SerialIO/) |


Nightly build packages:  https://www.myget.org/F/supersocket/api/v3/index.json

---

## SuperSocket 2.0 Roadmap:


- 2023:
    - More documents
    - Performance test/tuning
    - Fix issues of the existing features
    - Other features requested by users
    - Stable release
