# SuperSocket LiveChat Sample

A compl### WebSocket Protocol

The chat uses a simple text-based protocol:

- **Connect**: `CON <username>` - Join the chat room with a username
- **Message**: `MSG <message>` - Send a message to all users in the room

### Example WebSocket Session

```javascript
// Connect to WebSocket (HTTP only for development)
const ws = new WebSocket('ws://localhost:4040');

// Join chat
ws.send('CON Alice');

// Send message
ws.send('MSG Hello everyone!');
```

**Note**: For development, only the HTTP WebSocket endpoint (ws://localhost:4040) is enabled to avoid SSL certificate issues. In production, you can enable the HTTPS WebSocket endpoint by adding the SSL listener configuration back to appsettings.json.application demonstrating how to build live chat functionality using SuperSocket WebSocket server with ASP.NET Core and Angular.

## Features

- **Real-time WebSocket Communication**: Uses SuperSocket WebSocket server for bidirectional communication
- **Command-based Architecture**: Implements chat commands (CON for connect, MSG for message)
- **Room Management**: Supports user rooms with join/leave notifications
- **Angular Frontend**: Modern SPA with Material Design components
- **HTTP WebSocket Support**: WebSocket communication over HTTP (port 4040)

## Getting Started

### Prerequisites

- .NET 6.0 or later
- Node.js (for Angular development)

### Running the Application

1. **Build the Angular frontend**:
   ```bash
   cd ClientApp
   npm install
   npm run build
   cd ..
   ```

2. **Run the application**:
   ```bash
   dotnet run
   ```

3. **Access the application**:
   - Web interface: http://localhost:5000 or https://localhost:5001
   - WebSocket endpoint: ws://localhost:4040 (HTTP only for development)

## WebSocket Protocol

The chat uses a simple text-based protocol:

- **Connect**: `CON <username>` - Join the chat room with a username
- **Message**: `MSG <message>` - Send a message to all users in the room

### Example WebSocket Session

```javascript
// Connect to WebSocket
const ws = new WebSocket('ws://localhost:4040');

// Join chat
ws.send('CON Alice');

// Send message
ws.send('MSG Hello everyone!');
```

## Architecture

- **ChatSession**: WebSocket session with user information
- **RoomService**: Manages users and message broadcasting
- **Commands**: CON and MSG command handlers for chat operations
- **Angular Frontend**: SPA with routing and Material UI components

## Development

For development with Angular live reload:

1. Start the .NET backend: `dotnet run`
2. In another terminal, start Angular dev server: `cd ClientApp && npm start`
3. Angular dev server will run on http://localhost:4200 with proxy to backend

## Recent Updates

✅ **Fixed SPA Services Issue**: Removed deprecated `Microsoft.AspNetCore.SpaServices.Extensions` and modernized the approach to serve Angular static files directly.

The sample now works correctly with modern ASP.NET Core versions without deprecated SPA services.