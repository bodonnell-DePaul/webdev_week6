# Real-Time Web Technologies Comparison

This document provides a side-by-side comparison of the three main real-time web technologies covered in our lecture: WebSockets, SignalR, and Server-Sent Events (SSE).

## Setup Comparison

### Backend Configuration

| Technology | Backend Setup | Dependencies | URL Pattern |
|------------|--------------|--------------|-------------|
| **WebSockets** | `app.UseWebSockets()` | None (built into .NET) | `ws://` or `wss://` |
| **SignalR** | `builder.Services.AddSignalR()` <br> `app.MapHub<BookHub>("/bookHub")` | `@microsoft/signalr` (client) | HTTP endpoint `/bookHub` |
| **SSE** | Custom response headers <br> `Content-Type: text/event-stream` | None (native browser support) | HTTP endpoint `/api/books/events` |

### Frontend Dependencies

| Technology | NPM Package | Browser Support | Fallback Options |
|------------|------------|-----------------|-----------------|
| **WebSockets** | None (native) | All modern browsers | Need manual implementation |
| **SignalR** | `@microsoft/signalr` | All browsers via package | Automatic (Long Polling) |
| **SSE** | None (native) | Most modern browsers | EventSource polyfill |

## Connection Management

| Feature | WebSockets | SignalR | SSE |
|---------|-----------|---------|-----|
| **Connection Direction** | Bidirectional | Bidirectional | Server → Client only |
| **Auto Reconnect** | No (manual) | Yes (built-in) | Yes (built-in) |
| **Connection State** | Manual tracking | Built-in states | Manual tracking |
| **Multiple Connections** | Manual pool | Built-in groups | Manual tracking |
| **Authentication** | Manual | Integrated | HTTP auth |

## Message Patterns

| Pattern | WebSockets | SignalR | SSE |
|---------|-----------|---------|-----|
| **Broadcast to All** | Manual send to all | `Clients.All` | Send to all responses |
| **Group Messaging** | Manual tracking | `Clients.Group()` | Not built-in |
| **Individual Messaging** | Manual tracking | `Clients.Client(id)` | Not built-in |
| **RPC Style** | Manual implementation | Hub methods | Not supported |

## Message Format

| Technology | Format | Serialization | Message Types |
|------------|--------|---------------|--------------|
| **WebSockets** | Custom (typically JSON) | Manual `JSON.stringify()` | Custom fields |
| **SignalR** | Handled by library | Automatic | Method name + params |
| **SSE** | `event: type` <br> `data: payload` | Manual JSON in `data` field | Named events |

## Code Examples

### 1. Establishing a Connection

#### WebSockets

```typescript
// Client
const ws = new WebSocket('ws://localhost:5000/books-ws');

ws.onopen = () => {
  console.log('Connection established');
};

ws.onclose = () => {
  console.log('Connection closed');
};

ws.onerror = (error) => {
  console.error('WebSocket error:', error);
};
```

```csharp
// Server
app.UseWebSockets();
app.Map("/books-ws", async (HttpContext context) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    await HandleWebSocketConnection(webSocket);
});
```

#### SignalR

```typescript
// Client
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
    .withUrl('/bookHub')
    .withAutomaticReconnect()
    .build();

connection.start()
    .then(() => console.log('SignalR Connected'))
    .catch(err => console.error('SignalR Connection Error:', err));
```

```csharp
// Server
builder.Services.AddSignalR();
app.MapHub<BookHub>("/bookHub");

public class BookHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Connection handling
        await base.OnConnectedAsync();
    }
}
```

#### Server-Sent Events (SSE)

```typescript
// Client
const eventSource = new EventSource('/api/books/events');

eventSource.onopen = () => {
  console.log('SSE connection opened');
};

eventSource.onerror = (error) => {
  console.error('SSE connection error:', error);
};
```

```csharp
// Server
app.MapGet("/api/books/events", async (HttpContext context) =>
{
    // Set SSE headers
    context.Response.Headers.Append("Content-Type", "text/event-stream");
    context.Response.Headers.Append("Cache-Control", "no-cache");
    context.Response.Headers.Append("Connection", "keep-alive");
    
    // Keep connection open and send events
    while (!context.RequestAborted.IsCancellationRequested)
    {
        // Events are sent here
        await Task.Delay(TimeSpan.FromSeconds(30));
    }
});
```

### 2. Sending & Receiving Messages

#### WebSockets

```typescript
// Client - Sending
ws.send(JSON.stringify({ 
  type: 'subscribe', 
  channel: 'book-updates' 
}));

// Client - Receiving
ws.onmessage = (event) => {
  const data = JSON.parse(event.data);
  console.log('Received:', data);
};
```

```csharp
// Server - Receiving
var buffer = new byte[4096];
var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

// Server - Sending
var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(responseObject));
await webSocket.SendAsync(
    new ArraySegment<byte>(responseBytes),
    WebSocketMessageType.Text,
    true,
    CancellationToken.None);
```

#### SignalR

```typescript
// Client - Invoking server methods
connection.invoke("SubscribeToBook", "book-123")
    .catch(err => console.error('Error invoking method:', err));

// Client - Receiving server messages
connection.on("BookUpdated", (book) => {
    console.log("Book updated:", book);
});
```

```csharp
// Server - Receiving client calls
public Task SubscribeToBook(string bookId)
{
    // Add client to book subscription
    return Task.CompletedTask;
}

// Server - Sending to clients
await Clients.Group("BookUpdates").SendAsync("BookUpdated", book);
await Clients.Client(connectionId).SendAsync("BookUpdated", book);
```

#### Server-Sent Events (SSE)

```typescript
// Client - Receiving named events
eventSource.addEventListener('book-added', (event) => {
    const book = JSON.parse(event.data);
    console.log('Book added:', book);
});

eventSource.addEventListener('book-updated', (event) => {
    const book = JSON.parse(event.data);
    console.log('Book updated:', book);
});

// Client - Receiving default events
eventSource.onmessage = (event) => {
    const data = JSON.parse(event.data);
    console.log('Message received:', data);
};
```

```csharp
// Server - Sending events
await context.Response.WriteAsync($"event: book-updated\n");
await context.Response.WriteAsync($"data: {JsonSerializer.Serialize(book)}\n\n");
await context.Response.Body.FlushAsync();
```

### 3. Error Handling & Reconnection

#### WebSockets

```typescript
// Client
let reconnectAttempts = 0;
const maxReconnectAttempts = 5;

function connectWebSocket() {
    ws = new WebSocket('ws://localhost:5000/books-ws');
    
    ws.onclose = (event) => {
        if (event.code !== 1000 && reconnectAttempts < maxReconnectAttempts) {
            reconnectAttempts++;
            const timeout = Math.min(1000 * Math.pow(2, reconnectAttempts), 30000);
            setTimeout(connectWebSocket, timeout);
        }
    };
    
    ws.onopen = () => {
        reconnectAttempts = 0;
        // Resubscribe to channels after reconnect
    };
}
```

#### SignalR

```typescript
// Client - Built-in reconnection
const connection = new signalR.HubConnectionBuilder()
    .withUrl('/bookHub')
    .withAutomaticReconnect([0, 2000, 10000, 30000]) // Retry delays in ms
    .build();

// Monitor connection state
connection.onreconnecting(error => {
    console.log('Connection lost, reconnecting...', error);
});

connection.onreconnected(connectionId => {
    console.log('Connection reestablished. Connected with ID', connectionId);
    // Resubscribe or refresh data
});

connection.onclose(error => {
    console.log('Connection closed permanently', error);
});
```

#### Server-Sent Events (SSE)

```typescript
// Client - Built-in reconnection
// EventSource automatically reconnects with exponential backoff
const eventSource = new EventSource('/api/books/events');

eventSource.onerror = (event) => {
    if (eventSource.readyState === EventSource.CONNECTING) {
        console.log('Reconnecting...');
    } else {
        console.error('SSE error:', event);
    }
};
```

## Choosing The Right Technology

| Use Case | Recommended Technology | Reasoning |
|----------|----------------------|-----------|
| **Chat application** | WebSockets or SignalR | Needs bidirectional communication |
| **Live dashboard** | SSE or SignalR | Server-to-client updates are primary |
| **Collaborative editing** | WebSockets or SignalR | Requires low latency, bidirectional |
| **Notifications** | SSE | Simple one-way updates |
| **.NET backend** | SignalR | Best integration with .NET |
| **Simple implementation** | SSE | Easiest to implement |
| **Maximum control** | WebSockets | Most flexible but most work |
| **Cross-platform** | SignalR | Works well with any client |

## Performance Considerations

| Aspect | WebSockets | SignalR | SSE |
|--------|-----------|---------|-----|
| **Connection Overhead** | Low after initial handshake | Medium | Medium |
| **Message Size** | Very small | Small | Larger (HTTP headers) |
| **Latency** | Lowest | Low | Medium |
| **Server Scalability** | Requires specialized handling | Good with backplane | Good |
| **Connection Limits** | Browser limits connections | Managed by library | Browser limits connections |

## Implementation Notes

### WebSockets
- Requires extra care for reliable connections
- Best raw performance
- Most work to implement properly
- Good for high-frequency updates

### SignalR
- Most developer-friendly
- Best for .NET backends
- Handles reconnection automatically
- Provides groups and user targeting

### Server-Sent Events
- Simplest to implement
- Works over standard HTTP
- Only server-to-client
- Good for infrequent updates

## Browser Support

| Technology | Chrome | Firefox | Safari | Edge | IE |
|------------|--------|---------|--------|------|-----|
| **WebSockets** | ✅ | ✅ | ✅ | ✅ | 10+ |
| **SSE** | ✅ | ✅ | ✅ | ✅ | ❌ |
| **SignalR** | ✅ | ✅ | ✅ | ✅ | ✅* |

*With polyfills and fallbacks built into the client
