# Real-Time Web Technologies Cheat Sheet

## WebSockets

### Client Setup (Browser)
```javascript
const ws = new WebSocket('ws://example.com/socket');
ws.onopen = () => console.log('Connected');
ws.onmessage = (event) => console.log('Received:', event.data);
ws.onclose = () => console.log('Disconnected');
ws.onerror = (error) => console.error('Error:', error);

// Send data
ws.send(JSON.stringify({ type: 'hello', data: 'world' }));
```

### Server Setup (.NET)
```csharp
// Program.cs
app.UseWebSockets();

app.Map("/socket", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }
    
    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    await HandleWebSocket(webSocket);
});

// Receiving data
var buffer = new byte[1024 * 4];
var result = await webSocket.ReceiveAsync(
    new ArraySegment<byte>(buffer), 
    CancellationToken.None);

// Sending data
var responseBytes = Encoding.UTF8.GetBytes(jsonString);
await webSocket.SendAsync(
    new ArraySegment<byte>(responseBytes),
    WebSocketMessageType.Text,
    true,
    CancellationToken.None);
```

## SignalR

### Client Setup (Browser)
```javascript
// Install: npm install @microsoft/signalr
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/myhub")
    .withAutomaticReconnect()
    .build();

// Receive messages
connection.on("ReceiveMessage", (user, message) => {
    console.log(`${user}: ${message}`);
});

// Start the connection
await connection.start();

// Send messages
await connection.invoke("SendMessage", user, message);
```

### Server Setup (.NET)
```csharp
// Program.cs
builder.Services.AddSignalR();
app.MapHub<ChatHub>("/myhub");

// ChatHub.cs
public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
    
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "MainGroup");
        await base.OnConnectedAsync();
    }
}

// Send to specific clients
await Clients.All.SendAsync("method", args);          // All clients
await Clients.Caller.SendAsync("method", args);       // Calling client
await Clients.Others.SendAsync("method", args);       // All except caller
await Clients.Client(connectionId).SendAsync(...);    // Specific client
await Clients.Group("group").SendAsync(...);          // Group of clients
```

## Server-Sent Events (SSE)

### Client Setup (Browser)
```javascript
const eventSource = new EventSource("/events");

// Basic handler
eventSource.onmessage = (event) => {
    console.log("Data:", event.data);
};

// Named events
eventSource.addEventListener("update", (event) => {
    console.log("Update:", JSON.parse(event.data));
});

eventSource.addEventListener("error", (event) => {
    console.error("Error:", event);
});

// Close connection
eventSource.close();
```

### Server Setup (.NET)
```csharp
// Program.cs
app.MapGet("/events", HandleSSE);

// Handler
async Task HandleSSE(HttpContext context)
{
    // Set SSE headers
    context.Response.Headers.Append("Content-Type", "text/event-stream");
    context.Response.Headers.Append("Cache-Control", "no-cache");
    context.Response.Headers.Append("Connection", "keep-alive");
    
    // Send events
    while (!context.RequestAborted.IsCancellationRequested)
    {
        // Send a named event
        await context.Response.WriteAsync($"event: update\n");
        await context.Response.WriteAsync($"data: {JsonSerializer.Serialize(data)}\n\n");
        await context.Response.Body.FlushAsync();
        
        await Task.Delay(TimeSpan.FromSeconds(5), context.RequestAborted);
    }
}
```

## Technology Selection Guide

| Feature | WebSockets | SignalR | SSE |
|---------|:----------:|:-------:|:---:|
| Bidirectional | ✅ | ✅ | ❌ |
| Auto Reconnect | ❌ | ✅ | ✅ |
| Browser Support | Excellent | Excellent (via polyfills) | Good |
| Complexity | Medium | Low | Very Low |
| Transport Fallback | ❌ | ✅ | ❌ |
| .NET Integration | Manual | Excellent | Manual |
| Group Support | Manual | Built-in | Manual |
| Message Types | Manual | Method-based | Named Events |
| Connection Overhead | Low | Medium | Medium |
| Best For | High-frequency updates, custom protocols | Enterprise apps, .NET backends | Simple one-way updates |

## Common Patterns

### Reconnection Logic (WebSockets)
```javascript
function connect() {
    const ws = new WebSocket(url);
    
    ws.onclose = (event) => {
        if (event.code !== 1000) {  // Not a clean close
            setTimeout(() => {
                console.log('Reconnecting...');
                connect();
            }, 3000);  // Wait 3 seconds before reconnecting
        }
    };
    
    // Other handlers...
}
```

### Connection State Management (All)
```javascript
const [connectionState, setConnectionState] = useState('disconnected');

// Update UI based on connection state
<div className={`status-indicator ${connectionState}`}>
    {connectionState === 'connected' ? 'Online' : 'Offline'}
</div>
```

### Message Type Handling (WebSockets)
```javascript
ws.onmessage = (event) => {
    const message = JSON.parse(event.data);
    
    switch (message.type) {
        case 'update':
            handleUpdate(message.data);
            break;
        case 'error':
            handleError(message.data);
            break;
        default:
            console.log('Unknown message type:', message.type);
    }
};
```

## Security Considerations

1. **Authentication & Authorization**
   - Always validate user identity and permissions
   - Use token-based auth for WebSockets and SSE
   - SignalR integrates with .NET auth systems

2. **Input Validation**
   - Validate all incoming messages
   - Use strong typing when possible
   - Protect against injection attacks

3. **Rate Limiting**
   - Implement message rate limits
   - Prevent connection flooding
   - Consider timeouts for inactive connections

4. **HTTPS/WSS**
   - Always use secure protocols in production
   - wss:// instead of ws://
   - https:// for SSE

## Performance Tips

1. **Message Size**
   - Keep payloads small
   - Consider compression for larger data
   - Use binary formats for efficiency (WebSockets)

2. **Connection Management**
   - Limit connections per client
   - Implement graceful degradation
   - Use connection pools on server

3. **Scaling**
   - Use a backplane for SignalR (Redis/SQL Server)
   - Consider sticky sessions for WebSockets
   - Implement proper cleanup of dead connections
