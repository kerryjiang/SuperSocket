# SuperSocket

[![Build](https://github.com/kerryjiang/SuperSocket/workflows/build/badge.svg)](https://github.com/kerryjiang/SuperSocket/actions?query=workflow%3Abuild)
[![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.svg?style=flat)](https://www.nuget.org/packages/SuperSocket/)
[![NuGet](https://img.shields.io/nuget/dt/SuperSocket.svg)](https://www.nuget.org/packages/SuperSocket)
[![Badge](https://img.shields.io/badge/link-996.icu-red.svg)](https://996.icu/#/en_US)


**SuperSocket** 是一个用于 .NET 的高性能、可扩展的套接字服务器应用程序框架。它为构建自定义网络通信应用程序提供了强大的架构，支持包括 TCP、UDP 和 WebSocket 在内的多种协议。

- **项目主页**:		[https://www.supersocket.net/](https://www.supersocket.net/)
- **文档**:		[https://docs.supersocket.net/](https://docs.supersocket.net/)
- **License**: 				[https://www.apache.org/licenses/LICENSE-2.0](https://www.apache.org/licenses/LICENSE-2.0)

***SuperSocket 的主要特点包括：***
1. **灵活的管道架构：**
    SuperSocket 实现了基于管道的处理模型，通过可自定义的过滤器高效处理传入数据。

2. **协议抽象：**
    该框架抽象了底层套接字操作，并为实现各种协议提供了简洁的接口。它内置支持 TCP、UDP、WebSocket 以及自定义协议。

3. **中间件支持：**
    可扩展的中间件系统，允许对连接和数据包进行自定义处理。

4. **会话管理：**
    SuperSocket 提供全面的会话处理功能，管理从建立到终止的连接生命周期。

5. **命令处理系统：**
    基于命令的处理模型，高效处理客户端请求。

6. **WebSocket 支持：**
    完整实现 WebSocket 协议，包括压缩等扩展功能。

7. **现代 .NET 集成：**
    SuperSocket 专为现代 .NET 构建，与 .NET 生态系统中的依赖注入、配置和日志记录功能无缝集成。

8. **跨平台：**
    作为 .NET 库，SuperSocket 可在 .NET 支持的所有平台上运行。

8. **客户端支持：**
    该框架包含用于建立与套接字服务器连接的客户端组件，包括代理功能。

9. **高性能：**
    SuperSocket 通过缓冲池和最小化内存分配，设计用于高吞吐量和低延迟场景，内存使用高效。


***SuperSocket 适用于广泛的应用场景：***
* 包括实时通信系统
* IoT 设备连接
* 游戏服务器
* 聊天应用程序
* 以及任何需要自定义网络协议的场景

---

##### Nuget Package

| Package | MyGet 测试版发布 | NuGet 稳定版发布 |
| :------|:------------:|:------------:|
| **SuperSocket.ProtoBase** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.ProtoBase)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.ProtoBase) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.ProtoBase.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.ProtoBase/)|
| **SuperSocket.Primitives** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Primitives)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Primitives) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Primitives.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Primitives/)|
| **SuperSocket.Connection** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Connection)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Connection) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Connection.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Connection/)|
| **SuperSocket.Kestrel** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Kestrel)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Kestrel) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Kestrel.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Kestrel/)|
| **SuperSocket.Server** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Server)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Server) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Server.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Server/)|
| **SuperSocket.Server.Abstractions** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Server.Abstractions)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Server.Abstractions) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Server.Abstractions.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Server.Abstractions/)|
| **SuperSocket.Command** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Command)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Command) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Command.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Command/)|
| **SuperSocket.Client** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Client)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Client) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Client.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Client/)|
| **SuperSocket.Client.Proxy** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Client.Proxy)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Client.Proxy) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Client.Proxy.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Client.Proxy/)|
| **SuperSocket.WebSocket** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.WebSocket)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.WebSocket) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.WebSocket.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.WebSocket/)|
| **SuperSocket.WebSocket.Server** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.WebSocket.Server)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.WebSocket.Server) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.WebSocket.Server.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.WebSocket.Server/)|
| **SuperSocket.Udp** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.Udp)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.Udp) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.Udp.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.Udp/)|
| **[SuperSocket.SerialIO](https://github.com/SuperSocket/SuperSocket.SerialIO)** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.SerialIO)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.SerialIO) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.SerialIO.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.SerialIO/)|
| **[SuperSocket.ProtoBuf](https://github.com/SuperSocket/SuperSocket.ProtoBuf)** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.ProtoBuf)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.ProtoBuf) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.ProtoBuf.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.ProtoBuf/)|
| **[SuperSocket.MessagePack](https://github.com/SuperSocket/SuperSocket.MessagePack)** | [![MyGet Version](https://img.shields.io/myget/supersocket/vpre/SuperSocket.MessagePack)](https://www.myget.org/feed/supersocket/package/nuget/SuperSocket.MessagePack) | [![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.MessagePack.svg?style=flat)](https://www.nuget.org/packages/SuperSocket.MessagePack/)|


Nightly build packages:  https://www.myget.org/F/supersocket/api/v3/index.json

---

## SuperSocket 2.0 路线图：

- 2025:
    - More documents
    - Performance test/tuning
    - Fix issues of the existing features
    - Other features requested by users