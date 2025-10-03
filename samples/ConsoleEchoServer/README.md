# Console Echo Server Sample

This sample demonstrates how to use SuperSocket with stdin/stdout for console-based communication.

## Overview

The Console Echo Server shows how to:
- Use SuperSocket with console input/output streams
- Create a simple line-based protocol
- Handle text commands interactively
- Provide a command-line interface using SuperSocket's architecture

## Features

- **Console Integration**: Reads from stdin and writes to stdout
- **Interactive Commands**: Supports various text commands
- **Line-based Protocol**: Uses `\r\n` as message delimiter
- **Session Management**: Proper session lifecycle handling

## Available Commands

- `help` - Show available commands
- `time` - Display current server time
- `echo <message>` - Echo back the provided message
- `quit` or `exit` - Shutdown the server

## Running the Sample

```bash
cd samples/ConsoleEchoServer
dotnet run
```

## Usage Example

```
Console Echo Server starting...
This server reads from stdin and writes to stdout.
Type commands and press Enter. Type 'quit' to exit.
----------------------------------------
Welcome to Console Echo Server!
Type 'quit' to exit, 'help' for commands.

help
Available commands:
  help - Show this help message
  time - Show current time
  echo <message> - Echo back the message
  quit/exit - Close the server

echo Hello World
Echo: Hello World

time
Current time: 2025-10-03 14:30:15

quit
Server shutting down...
```

## Implementation Details

### Console Connection
- Uses `ConsoleConnection` which wraps `Console.In` and `Console.Out` streams
- Provides bidirectional communication through standard streams
- Handles stream lifecycle properly without disposing system streams

### Protocol
- Implements `LinePipelineFilter` for line-based text processing
- Uses `\r\n` as message terminator
- Returns `StringPackageInfo` for easy text handling

### Session Handling
- Custom `ConsoleSession` class extends `AppSession`
- Provides welcome message on connection
- Handles session cleanup on disconnection

## Use Cases

This pattern is useful for:
- Command-line tools that need protocol processing
- Development and debugging tools
- Console-based administration interfaces
- Interactive testing utilities
- Pipe-based communication scenarios

## Integration with Other Streams

The console connection can be adapted to work with other streams:
- File streams for batch processing
- Named pipes for inter-process communication
- Network streams for debugging
- Custom streams for specialized scenarios