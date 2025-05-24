# SuperSocket

[![Build](https://github.com/kerryjiang/SuperSocket/workflows/build/badge.svg)](https://github.com/kerryjiang/SuperSocket/actions?query=workflow%3Abuild)
[![NuGet Version](https://img.shields.io/nuget/vpre/SuperSocket.svg?style=flat)](https://www.nuget.org/packages/SuperSocket/)
[![NuGet](https://img.shields.io/nuget/dt/SuperSocket.svg)](https://www.nuget.org/packages/SuperSocket)
[![Badge](https://img.shields.io/badge/link-996.icu-red.svg)](https://996.icu/#/en_US)


**SuperSocket** is a high-performance, extensible socket server application framework for .NET. It provides a robust architecture for building custom network communication applications with support for multiple protocols including TCP, UDP, and WebSocket.

- **Project homepage**:		[https://www.supersocket.net/](https://www.supersocket.net/)
- **Documentation**:		[https://docs.supersocket.net/](https://docs.supersocket.net/)
- **License**: 				[https://www.apache.org/licenses/LICENSE-2.0](https://www.apache.org/licenses/LICENSE-2.0)

***Key features of SuperSocket include:***
1. **Flexible Pipeline Architecture:**
    SuperSocket implements a pipeline-based processing model that allows for efficient handling of incoming data with customizable filters.

2. **Protocol Abstraction:**
    The framework abstracts away low-level socket operations and provides a clean interface for implementing various protocols through pipeline filters. It has built-in support for TCP, UDP, WebSocket, and even custom protocols.

3. **Middleware Support:**
    Extensible middleware system allowing for custom processing of connections and packages.

4. **Session Management:**
    SuperSocket provides comprehensive session handling capabilities, managing connection lifecycles from establishment to termination.

5. **Command Handling System:**
    Command-based processing model to handle client requests efficiently.

6. **WebSocket Support:**
    Full implementation of the WebSocket protocol with extensions like compression.

7. **Modern .NET Integration:**
    SuperSocket is built for modern .NET and integrates seamlessly with the dependency injection, configuration, and logging facilities in the .NET ecosystem.

8. **Cross-Platform:**
    As a .NET library, SuperSocket works across platforms supported by .NET.

8. **Client Support:**
    The framework includes client components for establishing connections to socket servers, including proxy capabilities.

9. **High Performance:**
    SuperSocket is designed for high throughput and low latency with efficient memory usage through buffer pooling and minimal allocations


***SuperSocket is suitable for a wide range of applications including:***
* Real-time communication systems
* IoT device connectivity
* Game servers
* Chat applications
* and any scenario requiring custom network protocols

---

##### Nuget Packages

| Package | MyGet Beta Release | NuGet Stable Release |
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

## SuperSocket 2.0 Roadmap:

- 2025:
    - More documents
    - Performance test/tuning
    - Fix issues of the existing features
    - Other features requested by users