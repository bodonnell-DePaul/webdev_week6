# webdev_week8

boddev: boost this prompt: I need to give a lecture on the creation of webpages that deal with real time data.  The type of data does not matter, it can be stock market, transit GPS data or any type of real time data streams.  Provide me with detailed descriptions of different technologies needed for real time web applications including Web Sockets and Signal R.  If there are additonal technologies to be detailed include those technologies.  Also include detailed examples in both TypeScript and dotnet

GitHub Copilot: Let me help you boost this prompt to get more detailed and structured content for your lecture on real-time web applications. 

The boosted prompt provides a more structured and comprehensive approach for your lecture on real-time web applications. Let me create detailed content for your lecture based on this improved structure.

# Real-Time Web Applications: Technologies and Implementation

## Introduction to Real-Time Web Applications

Real-time web applications provide immediate data updates to users without requiring manual refresh. Examples include:
- Stock market dashboards
- Live GPS tracking systems
- Collaborative editing tools
- Chat applications
- Live sports updates

## Core Technologies for Real-Time Web Communication

### 1. WebSockets

WebSockets provide a persistent, bidirectional communication channel between client and server.

#### Technical Details:
- Protocol: Uses ws:// or wss:// (secure) protocols
- Handshake: Starts with HTTP then upgrades to WebSocket
- Full-duplex: Both client and server can send messages at any time
- Low latency: Minimal overhead after connection establishment

#### TypeScript Client Example:

```typescript
// client.ts
class StockTickerClient {
  private socket: WebSocket | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectDelay = 2000;
  
  constructor(private url: string, private onMessageCallback: (data: any) => void) {}
  
  connect(): void {
    try {
      this.socket = new WebSocket(this.url);
      
      this.socket.onopen = () => {
        console.log('WebSocket connection established');
        this.reconnectAttempts = 0;
      };
      
      this.socket.onmessage = (event) => {
        const data = JSON.parse(event.data);
        this.onMessageCallback(data);
      };
      
      this.socket.onclose = (event) => {
        console.log(`WebSocket connection closed: ${event.code} ${event.reason}`);
        this.handleReconnect();
      };
      
      this.socket.onerror = (error) => {
        console.error('WebSocket error:', error);
      };
    } catch (error) {
      console.error('Failed to establish WebSocket connection:', error);
      this.handleReconnect();
    }
  }
  
  private handleReconnect(): void {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++;
      console.log(`Attempting to reconnect (${this.reconnectAttempts}/${this.maxReconnectAttempts})...`);
      setTimeout(() => this.connect(), this.reconnectDelay);
    } else {
      console.error('Max reconnection attempts reached');
    }
  }
  
  sendMessage(data: any): void {
    if (this.socket && this.socket.readyState === WebSocket.OPEN) {
      this.socket.send(JSON.stringify(data));
    } else {
      console.error('WebSocket is not connected');
    }
  }
  
  disconnect(): void {
    if (this.socket) {
      this.socket.close();
      this.socket = null;
    }
  }
}

// Usage
const stockClient = new StockTickerClient('wss://api.stockmarket.com/stream', (data) => {
  // Update UI with new stock data
  document.getElementById('price')!.textContent = `$${data.price}`;
  document.getElementById('change')!.textContent = `${data.change}%`;
});

stockClient.connect();

// Subscribe to specific stocks
stockClient.sendMessage({
  action: 'subscribe',
  symbols: ['AAPL', 'MSFT', 'GOOG']
});
```

#### .NET (C#) Server Example:

```csharp
// StockTickerWebSocketHandler.cs
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Collections.Concurrent;

public class StockTickerWebSocketHandler
{
    private static ConcurrentDictionary<string, WebSocket> _clients = new ConcurrentDictionary<string, WebSocket>();
    private static ConcurrentDictionary<string, HashSet<string>> _subscriptions = new ConcurrentDictionary<string, HashSet<string>>();

    public async Task HandleWebSocket(HttpContext context, WebSocket webSocket)
    {
        var clientId = Guid.NewGuid().ToString();
        _clients.TryAdd(clientId, webSocket);

        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {
            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            await ProcessMessage(clientId, message);

            // Get next message
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        // Remove client on disconnect
        _clients.TryRemove(clientId, out _);
        foreach (var symbol in _subscriptions.Keys)
        {
            if (_subscriptions.TryGetValue(symbol, out var subscribers))
            {
                subscribers.Remove(clientId);
            }
        }

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    private async Task ProcessMessage(string clientId, string message)
    {
        try
        {
            var request = JsonSerializer.Deserialize<SubscriptionRequest>(message);
            
            if (request?.Action == "subscribe" && request.Symbols != null)
            {
                foreach (var symbol in request.Symbols)
                {
                    if (!_subscriptions.ContainsKey(symbol))
                    {
                        _subscriptions[symbol] = new HashSet<string>();
                    }
                    _subscriptions[symbol].Add(clientId);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
        }
    }

    public static async Task BroadcastStockUpdate(StockUpdate update)
    {
        if (_subscriptions.TryGetValue(update.Symbol, out var subscribers))
        {
            var updateJson = JsonSerializer.Serialize(update);
            var bytes = Encoding.UTF8.GetBytes(updateJson);
            
            foreach (var clientId in subscribers)
            {
                if (_clients.TryGetValue(clientId, out var socket) && socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending to client {clientId}: {ex.Message}");
                    }
                }
            }
        }
    }
}

public class SubscriptionRequest
{
    public string Action { get; set; }
    public string[] Symbols { get; set; }
}

public class StockUpdate
{
    public string Symbol { get; set; }
    public decimal Price { get; set; }
    public decimal Change { get; set; }
    public DateTime Timestamp { get; set; }
}

// Program.cs setup
public static class WebSocketExtensions
{
    public static IApplicationBuilder UseStockTickerWebSockets(this IApplicationBuilder app)
    {
        app.UseWebSockets();
        
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/api/stocks/stream")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var handler = new StockTickerWebSocketHandler();
                    await handler.HandleWebSocket(context, webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await next();
            }
        });
        
        return app;
    }
}
```

### 2. SignalR

SignalR is a Microsoft library that simplifies real-time web functionality, abstracting over multiple transport methods (WebSockets, Server-Sent Events, Long Polling).

#### Key Features:
- Automatic transport negotiation
- Connection management
- Groups for broadcasts
- Client reconnection
- Scale-out support with backplanes

#### TypeScript Client Example:

```typescript
// signalr-client.ts
import * as signalR from "@microsoft/signalr";

class TransitTrackerClient {
  private connection: signalR.HubConnection;
  private vehicleMarkers: Map<string, any> = new Map(); // Store map markers

  constructor() {
    // Create the connection
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("/transitHub")
      .withAutomaticReconnect([0, 2000, 10000, 30000]) // Retry intervals in ms
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Set up event handlers
    this.setupEventHandlers();
  }

  private setupEventHandlers() {
    // Handle vehicle position updates
    this.connection.on("ReceiveVehicleUpdate", (data: VehicleUpdate) => {
      console.log(`Vehicle ${data.vehicleId} updated`, data);
      this.updateVehicleOnMap(data);
    });

    // Handle connection events
    this.connection.onreconnecting(error => {
      console.log(`Connection lost due to: ${error}. Reconnecting...`);
      this.setConnectionStatus("Reconnecting");
    });

    this.connection.onreconnected(connectionId => {
      console.log(`Connection reestablished. Connected with ID: ${connectionId}`);
      this.setConnectionStatus("Connected");
      // Re-subscribe to routes of interest
      this.subscribeToRoutes(this.activeRoutes);
    });

    this.connection.onclose(error => {
      console.log(`Connection closed due to: ${error}`);
      this.setConnectionStatus("Disconnected");
    });
  }

  private activeRoutes: string[] = [];

  async start(): Promise<void> {
    try {
      await this.connection.start();
      console.log("Connected to SignalR hub");
      this.setConnectionStatus("Connected");
    } catch (err) {
      console.error("Failed to connect:", err);
      this.setConnectionStatus("Failed to connect");
      // Retry after 5 seconds
      setTimeout(() => this.start(), 5000);
    }
  }

  async subscribeToRoutes(routeIds: string[]): Promise<void> {
    if (this.connection.state === signalR.HubConnectionState.Connected) {
      try {
        await this.connection.invoke("SubscribeToRoutes", routeIds);
        this.activeRoutes = routeIds;
        console.log(`Subscribed to routes: ${routeIds.join(", ")}`);
      } catch (error) {
        console.error("Error subscribing to routes:", error);
      }
    } else {
      console.warn("Cannot subscribe - connection not established");
    }
  }

  private updateVehicleOnMap(data: VehicleUpdate): void {
    // Update vehicle position on map (implementation depends on your map library)
    if (this.vehicleMarkers.has(data.vehicleId)) {
      // Update existing marker
      const marker = this.vehicleMarkers.get(data.vehicleId);
      marker.setPosition({ lat: data.latitude, lng: data.longitude });
      marker.setRotation(data.heading);
    } else {
      // Create new marker
      const marker = new VehicleMarker(data); // Assuming VehicleMarker is your map marker class
      this.vehicleMarkers.set(data.vehicleId, marker);
    }
  }

  private setConnectionStatus(status: string): void {
    const statusElement = document.getElementById("connection-status");
    if (statusElement) {
      statusElement.textContent = status;
      statusElement.className = `status-${status.toLowerCase()}`;
    }
  }

  stop(): void {
    this.connection.stop();
  }
}

interface VehicleUpdate {
  vehicleId: string;
  routeId: string;
  latitude: number;
  longitude: number;
  heading: number;
  speed: number;
  timestamp: string;
  status: 'OnRoute' | 'Delayed' | 'OutOfService';
}

// Usage
const transitTracker = new TransitTrackerClient();
transitTracker.start().then(() => {
  transitTracker.subscribeToRoutes(["route1", "route2", "express-downtown"]);
});
```

#### .NET (C#) SignalR Hub Example:

```csharp
// TransitHub.cs
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TransitHub : Hub
{
    private static readonly Dictionary<string, HashSet<string>> RouteSubscriptions = new Dictionary<string, HashSet<string>>();
    private static readonly Dictionary<string, List<string>> ClientSubscriptions = new Dictionary<string, List<string>>();
    
    public async Task SubscribeToRoutes(string[] routeIds)
    {
        if (routeIds == null || routeIds.Length == 0)
            return;
            
        var connectionId = Context.ConnectionId;
        
        if (!ClientSubscriptions.ContainsKey(connectionId))
        {
            ClientSubscriptions[connectionId] = new List<string>();
        }
        
        foreach (var routeId in routeIds)
        {
            // Add to route subscriptions
            if (!RouteSubscriptions.ContainsKey(routeId))
            {
                RouteSubscriptions[routeId] = new HashSet<string>();
            }
            
            RouteSubscriptions[routeId].Add(connectionId);
            
            // Track client subscriptions for cleanup
            ClientSubscriptions[connectionId].Add(routeId);
            
            // Add to SignalR group
            await Groups.AddToGroupAsync(connectionId, $"route-{routeId}");
        }
        
        await Clients.Caller.SendAsync("SubscriptionConfirmed", routeIds);
    }
    
    public async Task UnsubscribeFromRoutes(string[] routeIds)
    {
        var connectionId = Context.ConnectionId;
        
        foreach (var routeId in routeIds)
        {
            if (RouteSubscriptions.ContainsKey(routeId))
            {
                RouteSubscriptions[routeId].Remove(connectionId);
            }
            
            if (ClientSubscriptions.ContainsKey(connectionId))
            {
                ClientSubscriptions[connectionId].Remove(routeId);
            }
            
            await Groups.RemoveFromGroupAsync(connectionId, $"route-{routeId}");
        }
    }
    
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var connectionId = Context.ConnectionId;
        
        if (ClientSubscriptions.ContainsKey(connectionId))
        {
            foreach (var routeId in ClientSubscriptions[connectionId])
            {
                if (RouteSubscriptions.ContainsKey(routeId))
                {
                    RouteSubscriptions[routeId].Remove(connectionId);
                }
            }
            
            ClientSubscriptions.Remove(connectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}

// VehicleUpdateService.cs
public class VehicleUpdateService : BackgroundService
{
    private readonly IHubContext<TransitHub> _hubContext;
    private readonly ITransitDataProvider _dataProvider;
    private readonly ILogger<VehicleUpdateService> _logger;
    
    public VehicleUpdateService(
        IHubContext<TransitHub> hubContext,
        ITransitDataProvider dataProvider,
        ILogger<VehicleUpdateService> logger)
    {
        _hubContext = hubContext;
        _dataProvider = dataProvider;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates = await _dataProvider.GetVehicleUpdatesAsync();
                
                foreach (var update in updates)
                {
                    // Broadcast to all clients subscribed to this route
                    await _hubContext.Clients
                        .Group($"route-{update.RouteId}")
                        .SendAsync("ReceiveVehicleUpdate", update, stoppingToken);
                }
                
                // Wait before next update cycle
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in vehicle update service");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}

// Program.cs setup
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 32768; // 32 KB
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// When using Azure SignalR Service
// builder.Services.AddSignalR().AddAzureSignalR();

builder.Services.AddHostedService<VehicleUpdateService>();

// Configure endpoints
app.MapHub<TransitHub>("/transitHub");
```

### 3. Server-Sent Events (SSE)

Server-Sent Events provide a one-way channel for server-to-client updates. They're simpler than WebSockets but limited to server-to-client communication.

#### Key Features:
- Built on HTTP, no special protocol
- Automatic reconnection
- Event IDs and reconnection state
- Event types
- Native browser API through EventSource

#### TypeScript Client Example:

```typescript
// weather-sse-client.ts
class WeatherMonitorClient {
  private eventSource: EventSource | null = null;
  private retryCount = 0;
  private maxRetries = 5;
  private retryInterval = 3000;
  
  constructor(private url: string) {}
  
  connect(): void {
    if (!window.EventSource) {
      console.error('EventSource is not supported in this browser');
      return;
    }
    
    try {
      this.eventSource = new EventSource(this.url);
      
      this.eventSource.onopen = () => {
        console.log('SSE connection established');
        this.retryCount = 0;
        this.updateStatus('Connected');
      };
      
      this.eventSource.addEventListener('weather-update', (event) => {
        const weatherData = JSON.parse(event.data);
        this.updateWeatherDisplay(weatherData);
      });
      
      this.eventSource.addEventListener('alert', (event) => {
        const alertData = JSON.parse(event.data);
        this.showWeatherAlert(alertData);
      });
      
      this.eventSource.onerror = () => {
        this.handleError();
      };
    } catch (error) {
      console.error('Failed to establish SSE connection:', error);
      this.handleError();
    }
  }
  
  private handleError(): void {
    this.updateStatus('Connection lost');
    
    if (this.eventSource) {
      this.eventSource.close();
      this.eventSource = null;
    }
    
    if (this.retryCount < this.maxRetries) {
      this.retryCount++;
      this.updateStatus(`Reconnecting (${this.retryCount}/${this.maxRetries})...`);
      setTimeout(() => this.connect(), this.retryInterval);
    } else {
      this.updateStatus('Failed to connect after multiple attempts');
    }
  }
  
  private updateWeatherDisplay(data: WeatherData): void {
    document.getElementById('temperature')!.textContent = `${data.temperature}°C`;
    document.getElementById('humidity')!.textContent = `${data.humidity}%`;
    document.getElementById('wind')!.textContent = `${data.windSpeed} km/h ${data.windDirection}`;
    document.getElementById('updated')!.textContent = new Date(data.timestamp).toLocaleTimeString();
  }
  
  private showWeatherAlert(alert: WeatherAlert): void {
    const alertElement = document.createElement('div');
    alertElement.className = `alert alert-${alert.severity}`;
    alertElement.textContent = alert.message;
    
    document.getElementById('alerts-container')!.appendChild(alertElement);
    
    // Remove alert after a delay
    setTimeout(() => alertElement.remove(), 10000);
  }
  
  private updateStatus(status: string): void {
    document.getElementById('connection-status')!.textContent = status;
  }
  
  disconnect(): void {
    if (this.eventSource) {
      this.eventSource.close();
      this.eventSource = null;
      this.updateStatus('Disconnected');
    }
  }
}

interface WeatherData {
  temperature: number;
  humidity: number;
  windSpeed: number;
  windDirection: string;
  conditions: string;
  timestamp: string;
}

interface WeatherAlert {
  severity: 'info' | 'warning' | 'danger';
  message: string;
  timestamp: string;
}

// Usage
const weatherMonitor = new WeatherMonitorClient('/api/weather/events');
weatherMonitor.connect();
```

#### .NET (C#) SSE Server Example:

```csharp
// WeatherEventController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[Route("api/weather")]
public class WeatherEventController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    
    public WeatherEventController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }
    
    [HttpGet("events")]
    public async Task GetWeatherUpdates(CancellationToken cancellationToken)
    {
        var response = Response;
        response.Headers.Add("Content-Type", "text/event-stream");
        response.Headers.Add("Cache-Control", "no-cache");
        response.Headers.Add("Connection", "keep-alive");
        
        // Send initial data immediately
        var weatherData = await _weatherService.GetCurrentWeatherAsync();
        await SendEvent(response, "weather-update", weatherData);
        
        // Set up subscription to weather updates
        var updates = _weatherService.SubscribeToWeatherUpdates();
        
        try
        {
            // Send each weather update as an SSE event
            await foreach (var update in updates.WithCancellation(cancellationToken))
            {
                if (update is WeatherData data)
                {
                    await SendEvent(response, "weather-update", data);
                }
                else if (update is WeatherAlert alert)
                {
                    await SendEvent(response, "alert", alert);
                }
                
                await response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected, clean up resources
        }
    }
    
    private async Task SendEvent(HttpResponse response, string eventType, object data)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data);
        var bytes = Encoding.UTF8.GetBytes($"event: {eventType}\ndata: {json}\n\n");
        await response.Body.WriteAsync(bytes, 0, bytes.Length);
    }
}
```

### 4. WebRTC (Web Real-Time Communication)

WebRTC enables peer-to-peer real-time communication directly between browsers without requiring intermediary servers for data transfer. It's primarily used for video/audio streaming and data sharing.

#### Key Features:
- P2P communication
- Audio and video streams
- Data channels for arbitrary data
- NAT traversal
- End-to-end encryption

#### TypeScript Client Example (Data Channel):

```typescript
// webrtc-data-client.ts
class CollaborativeEditorClient {
  private localConnection: RTCPeerConnection | null = null;
  private dataChannel: RTCDataChannel | null = null;
  private signaling: SignalingService; // Custom signaling service via WebSocket/SignalR
  private documentId: string;
  
  constructor(documentId: string, signalingUrl: string) {
    this.documentId = documentId;
    this.signaling = new SignalingService(signalingUrl);
    this.setupSignaling();
  }
  
  private setupSignaling() {
    this.signaling.onMessage = async (message) => {
      try {
        if (message.type === 'offer' && message.target === this.peerId) {
          await this.handleOffer(message.data, message.sender);
        } else if (message.type === 'answer' && message.target === this.peerId) {
          await this.handleAnswer(message.data);
        } else if (message.type === 'ice-candidate' && message.target === this.peerId) {
          await this.handleIceCandidate(message.data);
        }
      } catch (error) {
        console.error('Error handling signaling message:', error);
      }
    };
  }
  
  private peerId = crypto.randomUUID();
  
  async createConnection(remotePeerId: string): Promise<void> {
    try {
      this.localConnection = new RTCPeerConnection({
        iceServers: [
          { urls: 'stun:stun.l.google.com:19302' },
          { urls: 'stun:stun1.l.google.com:19302' }
        ]
      });
      
      this.dataChannel = this.localConnection.createDataChannel('editor', {
        ordered: true
      });
      
      this.setupDataChannel();
      this.setupPeerConnection(remotePeerId);
      
      // Create and send offer
      const offer = await this.localConnection.createOffer();
      await this.localConnection.setLocalDescription(offer);
      
      this.signaling.send({
        type: 'offer',
        sender: this.peerId,
        target: remotePeerId,
        data: offer
      });
    } catch (error) {
      console.error('Error creating connection:', error);
    }
  }
  
  private setupDataChannel(): void {
    if (!this.dataChannel) return;
    
    this.dataChannel.onopen = () => {
      console.log('Data channel opened');
      this.onConnectionStateChange('connected');
    };
    
    this.dataChannel.onclose = () => {
      console.log('Data channel closed');
      this.onConnectionStateChange('disconnected');
    };
    
    this.dataChannel.onmessage = (event) => {
      const change = JSON.parse(event.data);
      this.onTextChange(change);
    };
    
    this.dataChannel.onerror = (error) => {
      console.error('Data channel error:', error);
    };
  }
  
  private setupPeerConnection(remotePeerId: string): void {
    if (!this.localConnection) return;
    
    this.localConnection.onicecandidate = (event) => {
      if (event.candidate) {
        this.signaling.send({
          type: 'ice-candidate',
          sender: this.peerId,
          target: remotePeerId,
          data: event.candidate
        });
      }
    };
    
    this.localConnection.ondatachannel = (event) => {
      this.dataChannel = event.channel;
      this.setupDataChannel();
    };
    
    this.localConnection.onconnectionstatechange = () => {
      console.log('Connection state:', this.localConnection?.connectionState);
    };
  }
  
  async handleOffer(offer: RTCSessionDescriptionInit, remotePeerId: string): Promise<void> {
    if (!this.localConnection) {
      this.localConnection = new RTCPeerConnection({
        iceServers: [
          { urls: 'stun:stun.l.google.com:19302' },
          { urls: 'stun:stun1.l.google.com:19302' }
        ]
      });
      
      this.setupPeerConnection(remotePeerId);
    }
    
    await this.localConnection.setRemoteDescription(new RTCSessionDescription(offer));
    const answer = await this.localConnection.createAnswer();
    await this.localConnection.setLocalDescription(answer);
    
    this.signaling.send({
      type: 'answer',
      sender: this.peerId,
      target: remotePeerId,
      data: answer
    });
  }
  
  async handleAnswer(answer: RTCSessionDescriptionInit): Promise<void> {
    if (this.localConnection) {
      await this.localConnection.setRemoteDescription(new RTCSessionDescription(answer));
    }
  }
  
  async handleIceCandidate(candidate: RTCIceCandidateInit): Promise<void> {
    if (this.localConnection) {
      try {
        await this.localConnection.addIceCandidate(new RTCIceCandidate(candidate));
      } catch (error) {
        console.error('Error adding ICE candidate:', error);
      }
    }
  }
  
  sendTextChange(change: TextChange): void {
    if (this.dataChannel && this.dataChannel.readyState === 'open') {
      this.dataChannel.send(JSON.stringify(change));
    }
  }
  
  close(): void {
    if (this.dataChannel) {
      this.dataChannel.close();
    }
    
    if (this.localConnection) {
      this.localConnection.close();
    }
    
    this.signaling.disconnect();
  }
  
  // Client-provided callbacks
  onConnectionStateChange: (state: 'connecting' | 'connected' | 'disconnected') => void = () => {};
  onTextChange: (change: TextChange) => void = () => {};
}

interface TextChange {
  position: number;
  insertedText?: string;
  deletedLength?: number;
  author: string;
  timestamp: number;
}

// Simplified signaling service implementation
class SignalingService {
  private socket: WebSocket;
  
  constructor(url: string) {
    this.socket = new WebSocket(url);
    
    this.socket.onopen = () => {
      console.log('Signaling connection established');
    };
    
    this.socket.onmessage = (event) => {
      const message = JSON.parse(event.data);
      this.onMessage(message);
    };
  }
  
  send(message: any): void {
    if (this.socket.readyState === WebSocket.OPEN) {
      this.socket.send(JSON.stringify(message));
    } else {
      console.error('Signaling connection not open');
    }
  }
  
  disconnect(): void {
    this.socket.close();
  }
  
  onMessage: (message: any) => void = () => {};
}
```

### 5. MQTT (Message Queuing Telemetry Transport)

MQTT is a lightweight publish-subscribe protocol designed for constrained devices and networks with high latency or limited bandwidth.

#### Key Features:
- Extremely lightweight
- Publish/Subscribe model
- QoS levels (0,1,2)
- Retained messages
- Last Will and Testament
- Persistent sessions

#### TypeScript Client Example (using MQTT.js):

```typescript
// mqtt-client.ts
import * as mqtt from 'mqtt';

class SmartHomeClient {
  private client: mqtt.MqttClient | null = null;
  private subscriptions: string[] = [];
  private deviceStates: Map<string, DeviceState> = new Map();
  
  constructor(private brokerUrl: string, private clientId: string) {}
  
  connect(): Promise<void> {
    return new Promise((resolve, reject) => {
      try {
        // Connect to MQTT broker
        this.client = mqtt.connect(this.brokerUrl, {
          clientId: this.clientId,
          clean: true,
          connectTimeout: 5000,
          username: 'user',  // If authentication is needed
          password: 'password',
          reconnectPeriod: 1000
        });
        
        // Set up event handlers
        this.client.on('connect', () => {
          console.log('Connected to MQTT broker');
          this.resubscribe();
          resolve();
        });
        
        this.client.on('message', (topic, payload) => {
          try {
            console.log(`Received message on ${topic}: ${payload.toString()}`);
            this.handleMessage(topic, payload.toString());
          } catch (error) {
            console.error('Error handling message:', error);
          }
        });
        
        this.client.on('error', (error) => {
          console.error('MQTT connection error:', error);
          reject(error);
        });
        
        this.client.on('offline', () => {
          console.log('MQTT client went offline');
        });
        
        this.client.on('reconnect', () => {
          console.log('Attempting to reconnect to MQTT broker');
        });
      } catch (error) {
        console.error('Failed to connect to MQTT broker:', error);
        reject(error);
      }
    });
  }
  
  private resubscribe(): void {
    if (!this.client?.connected) return;
    
    for (const topic of this.subscriptions) {
      this.client.subscribe(topic, (err) => {
        if (err) {
          console.error(`Failed to resubscribe to ${topic}:`, err);
        } else {
          console.log(`Resubscribed to ${topic}`);
        }
      });
    }
  }
  
  subscribe(topic: string): void {
    if (!this.client?.connected) {
      console.error('Cannot subscribe, client not connected');
      return;
    }
    
    this.client.subscribe(topic, (err) => {
      if (err) {
        console.error(`Failed to subscribe to ${topic}:`, err);
      } else {
        console.log(`Subscribed to ${topic}`);
        if (!this.subscriptions.includes(topic)) {
          this.subscriptions.push(topic);
        }
      }
    });
  }
  
  unsubscribe(topic: string): void {
    if (!this.client?.connected) return;
    
    this.client.unsubscribe(topic, (err) => {
      if (err) {
        console.error(`Failed to unsubscribe from ${topic}:`, err);
      } else {
        console.log(`Unsubscribed from ${topic}`);
        const index = this.subscriptions.indexOf(topic);
        if (index > -1) {
          this.subscriptions.splice(index, 1);
        }
      }
    });
  }
  
  publish(topic: string, message: string | object, retain: boolean = false): void {
    if (!this.client?.connected) {
      console.error('Cannot publish, client not connected');
      return;
    }
    
    const payload = typeof message === 'string' ? message : JSON.stringify(message);
    
    this.client.publish(topic, payload, { qos: 1, retain }, (err) => {
      if (err) {
        console.error(`Failed to publish to ${topic}:`, err);
      } else {
        console.log(`Published to ${topic}: ${payload}`);
      }
    });
  }
  
  private handleMessage(topic: string, message: string): void {
    // Handle different topic patterns
    if (topic.startsWith('home/devices/')) {
      const deviceId = topic.split('/')[2];
      
      try {
        const deviceState = JSON.parse(message) as DeviceState;
        this.deviceStates.set(deviceId, deviceState);
        
        // Notify UI of device state change
        this.onDeviceStateChanged(deviceId, deviceState);
      } catch (error) {
        console.error(`Error parsing device state for ${deviceId}:`, error);
      }
    } else if (topic === 'home/alerts') {
      try {
        const alert = JSON.parse(message) as HomeAlert;
        this.onAlertReceived(alert);
      } catch (error) {
        console.error('Error parsing alert:', error);
      }
    }
  }
  
  getDeviceState(deviceId: string): DeviceState | undefined {
    return this.deviceStates.get(deviceId);
  }
  
  getAllDeviceStates(): Map<string, DeviceState> {
    return new Map(this.deviceStates);
  }
  
  controlDevice(deviceId: string, command: DeviceCommand): void {
    const topic = `home/devices/${deviceId}/commands`;
    this.publish(topic, command);
  }
  
  disconnect(): void {
    if (this.client?.connected) {
      this.client.end();
      console.log('Disconnected from MQTT broker');
    }
  }
  
  // Event handlers to be implemented by consumers
  onDeviceStateChanged: (deviceId: string, state: DeviceState) => void = () => {};
  onAlertReceived: (alert: HomeAlert) => void = () => {};
}

interface DeviceState {
  type: 'light' | 'thermostat' | 'door' | 'camera';
  status: 'online' | 'offline';
  power: boolean;
  lastUpdated: string;
  [key: string]: any; // Additional device-specific properties
}

interface DeviceCommand {
  action: string;
  parameters?: Record<string, any>;
  timestamp: string;
}

interface HomeAlert {
  type: string;
  severity: 'info' | 'warning' | 'critical';
  message: string;
  deviceId?: string;
  timestamp: string;
}

// Usage
async function initSmartHome() {
  const client = new SmartHomeClient('wss://mqtt-broker.example.com:8083/mqtt', 'web-client-' + Date.now());
  
  client.onDeviceStateChanged = (deviceId, state) => {
    console.log(`Device ${deviceId} state changed:`, state);
    updateDeviceUI(deviceId, state);
  };
  
  client.onAlertReceived = (alert) => {
    showAlert(alert);
  };
  
  try {
    await client.connect();
    
    // Subscribe to topics
    client.subscribe('home/devices/+');
    client.subscribe('home/alerts');
    
    // Get initial state
    client.publish('home/system/ping', { clientId: client.clientId, timestamp: new Date().toISOString() });
  } catch (error) {
    console.error('Failed to initialize smart home client:', error);
  }
  
  return client;
}

function updateDeviceUI(deviceId: string, state: DeviceState) {
  const deviceElement = document.getElementById(`device-${deviceId}`);
  if (!deviceElement) return;
  
  deviceElement.querySelector('.status')!.textContent = state.status;
  deviceElement.querySelector('.power')!.textContent = state.power ? 'On' : 'Off';
  
  // Update device-specific UI elements
  if (state.type === 'thermostat') {
    deviceElement.querySelector('.temperature')!.textContent = `${state.temperature}°C`;
  } else if (state.type === 'light') {
    deviceElement.querySelector('.brightness')!.textContent = `${state.brightness}%`;
  }
}

function showAlert(alert: HomeAlert) {
  const alertsContainer = document.getElementById('alerts-container')!;
  
  const alertElement = document.createElement('div');
  alertElement.className = `alert alert-${alert.severity}`;
  alertElement.innerHTML = `
    <strong>${alert.type}</strong>: ${alert.message}
    <span class="timestamp">${new Date(alert.timestamp).toLocaleTimeString()}</span>
  `;
  
  alertsContainer.appendChild(alertElement);
  
  // Auto dismiss non-critical alerts
  if (alert.severity !== 'critical') {
    setTimeout(() => alertElement.remove(), 10000);
  }
}
```

#### .NET MQTT Client Example:

```csharp
// MqttService.cs
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class MqttService : IDisposable
{
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _options;
    private readonly ILogger<MqttService> _logger;
    
    public event EventHandler<DeviceStateChangedEventArgs> DeviceStateChanged;
    public event EventHandler<AlertReceivedEventArgs> AlertReceived;
    
    public MqttService(IConfiguration configuration, ILogger<MqttService> logger)
    {
        _logger = logger;
        
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();
        
        var brokerHost = configuration["Mqtt:BrokerHost"];
        var brokerPort = int.Parse(configuration["Mqtt:BrokerPort"]);
        var clientId = $"backend-service-{Guid.NewGuid()}";
        
        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(brokerHost, brokerPort)
            .WithClientId(clientId)
            .WithCredentials(configuration["Mqtt:Username"], configuration["Mqtt:Password"])
            .WithCleanSession()
            .Build();
        
        _mqttClient.UseConnectedHandler(async e =>
        {
            _logger.LogInformation("Connected to MQTT broker");
            
            // Subscribe to topics
            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic("home/devices/+")
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());
                
            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic("home/alerts")
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());
        });
        
        _mqttClient.UseDisconnectedHandler(e =>
        {
            _logger.LogWarning("Disconnected from MQTT broker: {Reason}", e.Reason);
            
            // Reconnect logic
            Task.Delay(TimeSpan.FromSeconds(5))
                .ContinueWith(_ => ConnectAsync())
                .ConfigureAwait(false);
        });
        
        _mqttClient.UseApplicationMessageReceivedHandler(e =>
        {
            try
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                
                _logger.LogDebug("Received message on topic {Topic}: {Payload}", topic, payload);
                
                HandleMessage(topic, payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling MQTT message");
            }
        });
    }
    
    public async Task ConnectAsync()
    {
        try
        {
            if (!_mqttClient.IsConnected)
            {
                await _mqttClient.ConnectAsync(_options, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to MQTT broker");
            throw;
        }
    }
    
    public async Task PublishAsync(string topic, object payload, bool retain = false)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(json)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(retain)
                .Build();
                
            await _mqttClient.PublishAsync(message, CancellationToken.None);
            _logger.LogDebug("Published to topic {Topic}: {Payload}", topic, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to topic {Topic}", topic);
            throw;
        }
    }
    
    private void HandleMessage(string topic, string payload)
    {
        // Handle device state updates
        if (topic.StartsWith("home/devices/"))
        {
            var deviceId = topic.Split('/')[2];
            
            try
            {
                var deviceState = JsonSerializer.Deserialize<DeviceState>(payload);
                
                if (deviceState != null)
                {
                    DeviceStateChanged?.Invoke(this, new DeviceStateChangedEventArgs(deviceId, deviceState));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing device state for device {DeviceId}", deviceId);
            }
        }
        // Handle alerts
        else if (topic == "home/alerts")
        {
            try
            {
                var alert = JsonSerializer.Deserialize<HomeAlert>(payload);
                
                if (alert != null)
                {
                    AlertReceived?.Invoke(this, new AlertReceivedEventArgs(alert));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing alert message");
            }
        }
    }
    
    public async Task DisconnectAsync()
    {
        if (_mqttClient.IsConnected)
        {
            await _mqttClient.DisconnectAsync();
        }
    }
    
    public void Dispose()
    {
        _mqttClient?.Dispose();
    }
}

public class DeviceStateChangedEventArgs : EventArgs
{
    public string DeviceId { get; }
    public DeviceState State { get; }
    
    public DeviceStateChangedEventArgs(string deviceId, DeviceState state)
    {
        DeviceId = deviceId;
        State = state;
    }
}

public class AlertReceivedEventArgs : EventArgs
{
    public HomeAlert Alert { get; }
    
    public AlertReceivedEventArgs(HomeAlert alert)
    {
        Alert = alert;
    }
}

public class DeviceState
{
    public string Type { get; set; }
    public string Status { get; set; }
    public bool Power { get; set; }
    public string LastUpdated { get; set; }
    public Dictionary<string, JsonElement> Properties { get; set; } = new();
}

public class HomeAlert
{
    public string Type { get; set; }
    public string Severity { get; set; }
    public string Message { get; set; }
    public string DeviceId { get; set; }
    public string Timestamp { get; set; }
}
```

### 6. Long Polling vs. Short Polling

While not ideal for truly real-time applications, polling techniques are still used in some contexts.

#### Short Polling:
- Client makes repeated HTTP requests at fixed intervals 
- Server responds immediately with current data
- Simple to implement, but inefficient
- High server load and bandwidth usage
- Latency determined by polling interval

#### Long Polling:
- Client makes HTTP request
- Server holds the request open until data is available or timeout occurs
- Client immediately makes a new request after receiving response
- Lower overhead than short polling
- Better latency than short polling, worse than WebSocket

#### TypeScript Long Polling Example:

```typescript
// long-polling-client.ts
class StockPricePoller {
  private pollingUrl: string;
  private timeout: number;
  private isPolling = false;
  private lastTimestamp: string | null = null;
  private retryDelay = 2000;
  private maxRetries = 5;
  private retryCount = 0;
  
  constructor(baseUrl: string, timeout: number = 60000) {
    this.pollingUrl = `${baseUrl}/api/stocks/prices`;
    this.timeout = timeout;
  }
  
  startPolling(): void {
    if (this.isPolling) return;
    
    this.isPolling = true;
    this.poll();
  }
  
  stopPolling(): void {
    this.isPolling = false;
  }
  
  private async poll(): Promise<void> {
    if (!this.isPolling) return;
    
    try {
      const url = this.lastTimestamp 
        ? `${this.pollingUrl}?lastTimestamp=${encodeURIComponent(this.lastTimestamp)}`
        : this.pollingUrl;
        
      const response = await fetch(url, {
        method: 'GET',
        headers: {
          'Accept': 'application/json',
          'Cache-Control': 'no-cache'
        },
        signal: AbortSignal.timeout(this.timeout)
      });
      
      if (response.status === 200) {
        const data = await response.json();
        this.retryCount = 0;
        
        // Process data
        if (data.prices && data.prices.length > 0) {
          this.onDataReceived(data.prices);
          this.lastTimestamp = data.timestamp;
        }
      } else if (response.status === 204) {
        // No new data, continue polling
        console.log('No new data available');
      } else {
        console.error(`Server returned error: ${response.status}`, await response.text());
        this.handleError();
      }
    } catch (error) {
      console.error('Polling error:', error);
      this.handleError();
    } finally {
      // Always queue up the next poll if still polling
      if (this.isPolling) {
        setTimeout(() => this.poll(), 0);
      }
    }
  }
  
  private handleError(): void {
    this.retryCount++;
    
    if (this.retryCount <= this.maxRetries) {
      // Exponential backoff
      const delay = this.retryDelay * Math.pow(1.5, this.retryCount - 1);
      console.log(`Retrying in ${delay}ms (attempt ${this.retryCount}/${this.maxRetries})`);
      
      setTimeout(() => {
        if (this.isPolling) {
          this.poll();
        }
      }, delay);
    } else {
      console.error('Max retry attempts reached, stopping polling');
      this.stopPolling();
      this.onError(new Error('Failed to connect after maximum retry attempts'));
    }
  }
  
  // Event handlers
  onDataReceived: (prices: StockPrice[]) => void = () => {};
  onError: (error: Error) => void = () => {};
}

interface StockPrice {
  symbol: string;
  price: number;
  change: number;
  changePercent: number;
  timestamp: string;
}
```

#### .NET (C#) Long Polling Server Example:

```csharp
// StockPricesController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[Route("api/stocks")]
public class StockPricesController : ControllerBase
{
    private readonly IStockPriceService _stockPriceService;
    private readonly ILogger<StockPricesController> _logger;
    
    public StockPricesController(IStockPriceService stockPriceService, ILogger<StockPricesController> logger)
    {
        _stockPriceService = stockPriceService;
        _logger = logger;
    }
    
    [HttpGet("prices")]
    public async Task<ActionResult<StockPriceResponse>> GetPrices(
        [FromQuery] string lastTimestamp = null, 
        [FromQuery] int timeoutSeconds = 30,
        CancellationToken cancellationToken = default)
    {
        // Parse the last timestamp if provided
        DateTime? lastUpdateTime = null;
        if (!string.IsNullOrEmpty(lastTimestamp) && DateTime.TryParse(lastTimestamp, out var parsedTime))
        {
            lastUpdateTime = parsedTime;
        }
        
        // Cap the timeout
        timeoutSeconds = Math.Min(60, Math.Max(1, timeoutSeconds));
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        
        try
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, 
                new CancellationTokenSource(timeout).Token);
                
            // Wait for new data or timeout
            var result = await _stockPriceService.GetPriceUpdatesAsync(lastUpdateTime, cts.Token);
            
            // If no updates and timeout/cancellation, return no content
            if (result.Prices.Count == 0)
            {
                return NoContent();
            }
            
            // Return the new data
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            // If cancellation was due to timeout, return no content
            if (!cancellationToken.IsCancellationRequested)
            {
                return NoContent();
            }
            
            // Otherwise, the client canceled the request
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock price updates");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}

public interface IStockPriceService
{
    Task<StockPriceResponse> GetPriceUpdatesAsync(DateTime? lastUpdateTime, CancellationToken cancellationToken);
}

public class StockPriceService : IStockPriceService
{
    private readonly IStockDataProvider _dataProvider;
    
    public StockPriceService(IStockDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }
    
    public async Task<StockPriceResponse> GetPriceUpdatesAsync(DateTime? lastUpdateTime, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<List<StockPrice>>();
        var registration = default(CancellationTokenRegistration);
        
        // Register for updates
        Action<List<StockPrice>> updateHandler = prices =>
        {
            tcs.TrySetResult(prices);
        };
        
        try
        {
            _dataProvider.StockPriceUpdated += updateHandler;
            
            // Register cancellation
            registration = cancellationToken.Register(() => tcs.TrySetCanceled());
            
            // Check if we already have new data
            var currentPrices = _dataProvider.GetLatestPrices(lastUpdateTime);
            if (currentPrices.Count > 0)
            {
                return new StockPriceResponse
                {
                    Prices = currentPrices,
                    Timestamp = DateTime.UtcNow.ToString("o")
                };
            }
            
            // Wait for updates or cancellation
            var prices = await tcs.Task;
            
            return new StockPriceResponse
            {
                Prices = prices,
                Timestamp = DateTime.UtcNow.ToString("o")
            };
        }
        finally
        {
            _dataProvider.StockPriceUpdated -= updateHandler;
            registration.Dispose();
        }
    }
}

public class StockPriceResponse
{
    public List<StockPrice> Prices { get; set; } = new List<StockPrice>();
    public string Timestamp { get; set; }
}

public class StockPrice
{
    public string Symbol { get; set; }
    public decimal Price { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercent { get; set; }
    public string Timestamp { get; set; }
}
```

## Architectural Considerations for Real-Time Applications

### 1. Scalability Patterns

#### Horizontal Scaling:
- Load balancing connections
- Using Redis or similar for shared state
- Sticky sessions vs. non-sticky
- SignalR backplane options (Redis, SQL Server, Azure Service Bus)

#### Example SignalR Backplane Configuration:

```csharp
// Program.cs in ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR()
    .AddStackExchangeRedis(options =>
    {
        options.Configuration.ChannelPrefix = "app-name";
        options.Configuration.DefaultDatabase = 5;
    });

// For Azure SignalR Service integration:
// builder.Services.AddSignalR().AddAzureSignalR();
```

### 2. Performance Optimization

#### Message Size and Frequency:
- Throttling and debouncing updates
- Delta updates vs. full state updates
- Binary protocols (MessagePack, Protobuf)

#### SignalR with MessagePack Example:

```csharp
// Server-side
builder.Services.AddSignalR()
    .AddMessagePackProtocol(options =>
    {
        options.SerializerOptions = MessagePackSerializerOptions.Standard
            .WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithSecurity(MessagePackSecurity.UntrustedData);
    });
    
// Client-side (TypeScript)
import * as signalR from "@microsoft/signalr";
import * as signalRMsgPack from "@microsoft/signalr-protocol-msgpack";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/dataHub")
    .withHubProtocol(new signalRMsgPack.MessagePackHubProtocol())
    .build();
```

### 3. Error Handling and Connection Management

- Reconnection strategies
- Progressive backoff
- Circuit breakers
- Fallback mechanisms

### 4. Event-Driven Architecture

- Decoupling components with message brokers
- Event sourcing for persistent history
- Command Query Responsibility Segregation (CQRS)

## Security Considerations

### 1. Authentication and Authorization

- JWT tokens with WebSockets
- SignalR authorization handlers
- Connection validation

#### SignalR Authorization Example:

```csharp
// Hub with authorization
[Authorize]
public class SecureDataHub : Hub
{
    [Authorize(Roles = "Admin")]
    public async Task SendAdminMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveAdminMessage", message);
    }
    
    [Authorize(Policy = "CanViewSensitiveData")]
    public async Task GetSensitiveData()
    {
        // Implementation
    }
}

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewSensitiveData", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("DataAnalyst", "Admin")
              .RequireClaim("DataAccess", "Sensitive"));
});
```

### 2. Data Validation

- Sanitizing user input
- Rate limiting
- Input size limitations

## Testing Real-Time Applications

### 1. Unit Testing

```typescript
// Example test for WebSocket client
import { WebSocketClient } from './websocket-client';
import WS from 'jest-websocket-mock';

describe('WebSocketClient', () => {
  let server: WS;
  let client: WebSocketClient;
  
  beforeEach(async () => {
    server = new WS('ws://localhost:1234');
    client = new WebSocketClient('ws://localhost:1234');
    await server.connected;
  });
  
  afterEach(() => {
    WS.clean();
  });
  
  test('sends message when connected', async () => {
    client.sendMessage({ type: 'test', data: 'hello' });
    await expect(server).toReceiveMessage('{"type":"test","data":"hello"}');
  });
  
  test('receives message and notifies listeners', async () => {
    const mockHandler = jest.fn();
    client.onMessage(mockHandler);
    
    server.send('{"event":"update","data":{"value":42}}');
    
    expect(mockHandler).toHaveBeenCalledWith({
      event: 'update',
      data: { value: 42 }
    });
  });
});
```

### 2. Integration Testing

```csharp
// SignalR Hub integration test
public class SignalRHubTests : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _httpClient;
    
    public SignalRHubTests()
    {
        // Set up test server with required services
        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.UseStartup<TestStartup>();
            });
            
        _server = new TestServer(hostBuilder);
        _httpClient = _server.CreateClient();
    }
    
    [Fact]
    public async Task ConnectAndSendMessage()
    {
        // Create SignalR connection
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/hubs/dataHub", options =>
            {
                options.HttpMessageHandlerFactory = _ => _server.CreateHandler();
            })
            .Build();
            
        // Set up receiving handler
        var receivedMessages = new List<string>();
        connection.On<string>("ReceiveMessage", message => receivedMessages.Add(message));
        
        // Start connection
        await connection.StartAsync();
        
        // Send a message
        await connection.InvokeAsync("SendMessage", "Test Message");
        
        // Wait briefly for message processing
        await Task.Delay(100);
        
        // Assert
        Assert.Contains("Test Message", receivedMessages);
    }
    
    public void Dispose()
    {
        _httpClient?.Dispose();
        _server?.Dispose();
    }
}
```

## Conclusion

Real-time web applications require careful consideration of:
1. Communication protocols (WebSockets, SignalR, SSE, WebRTC, MQTT)
2. Architecture patterns for scalability and performance
3. Proper error handling and reconnection strategies
4. Security concerns
5. Testing strategies

By understanding these technologies and patterns, you can build robust real-time applications capable of handling various use cases from chat and collaboration to data visualization and IoT device monitoring.


## Advanced WebSocket Usage Patterns

### Binary Data Transmission with WebSockets

While text-based JSON is common, WebSockets excel at binary data transmission for scenarios requiring higher performance:

```typescript
// Binary WebSocket client for streaming sensor data
class BinarySensorClient {
  private socket: WebSocket | null = null;
  private dataBuffer: ArrayBuffer[] = [];
  
  constructor(private url: string) {}
  
  connect(): void {
    this.socket = new WebSocket(this.url);
    this.socket.binaryType = 'arraybuffer'; // Set to receive binary data
    
    this.socket.onopen = () => {
      console.log('Binary WebSocket connection established');
      this.startSending();
    };
    
    this.socket.onmessage = (event) => {
      // Handle incoming binary data
      const data = event.data as ArrayBuffer;
      this.processIncomingData(data);
    };
  }
  
  private startSending(): void {
    // Send sensor readings every 100ms
    setInterval(() => {
      if (!this.socket || this.socket.readyState !== WebSocket.OPEN) return;
      
      // Create binary representation of sensor data
      const buffer = new ArrayBuffer(16); // 4 float32 values
      const view = new DataView(buffer);
      
      // Write sensor values (temperature, humidity, pressure, light level)
      view.setFloat32(0, this.getSensorValue('temperature'), true);
      view.setFloat32(4, this.getSensorValue('humidity'), true);
      view.setFloat32(8, this.getSensorValue('pressure'), true);
      view.setFloat32(12, this.getSensorValue('light'), true);
      
      this.socket.send(buffer);
    }, 100);
  }
  
  private processIncomingData(buffer: ArrayBuffer): void {
    const view = new DataView(buffer);
    
    // Extract control commands
    const commandId = view.getUint8(0);
    const parameter1 = view.getFloat32(1, true);
    const parameter2 = view.getFloat32(5, true);
    
    console.log(`Command received: ${commandId}, params: ${parameter1}, ${parameter2}`);
    this.executeCommand(commandId, parameter1, parameter2);
  }
  
  private getSensorValue(sensorType: string): number {
    // Simulated sensor readings
    switch(sensorType) {
      case 'temperature': return 22.5 + Math.random() * 0.5;
      case 'humidity': return 45 + Math.random() * 2;
      case 'pressure': return 1013 + Math.random() * 1;
      case 'light': return 450 + Math.random() * 10;
      default: return 0;
    }
  }
  
  private executeCommand(commandId: number, param1: number, param2: number): void {
    // Handle control commands
    console.log(`Executing command ${commandId} with params ${param1}, ${param2}`);
  }
}
```

### WebSocket Subprotocols

WebSockets support subprotocols for specific application requirements:

```typescript
// Client with MQTT over WebSockets
class MqttOverWebSocketClient {
  private socket: WebSocket | null = null;
  
  connect(): void {
    // Specify MQTT as a subprotocol
    this.socket = new WebSocket('wss://broker.example.com/mqtt', ['mqtt']);
    
    this.socket.onopen = () => {
      console.log('Connected with MQTT subprotocol');
      this.sendMqttConnect();
    };
    
    this.socket.onmessage = (event) => {
      const data = event.data;
      this.processMqttPacket(data);
    };
  }
  
  private sendMqttConnect(): void {
    // Construct MQTT CONNECT packet
    const clientId = 'web_client_' + Math.random().toString(16).substr(2, 8);
    const packet = this.buildConnectPacket(clientId);
    this.socket?.send(packet);
  }
  
  private buildConnectPacket(clientId: string): Uint8Array {
    // Simplified MQTT CONNECT packet construction
    const buffer = new Uint8Array(10 + clientId.length);
    // Fixed header
    buffer[0] = 0x10; // CONNECT packet type
    buffer[1] = 0x08 + clientId.length; // Remaining length
    // Variable header
    buffer[2] = 0x00; // Protocol name length MSB
    buffer[3] = 0x04; // Protocol name length LSB
    buffer[4] = 0x4D; // 'M'
    buffer[5] = 0x51; // 'Q'
    buffer[6] = 0x54; // 'T'
    buffer[7] = 0x54; // 'T'
    buffer[8] = 0x04; // Protocol version
    buffer[9] = 0x02; // Connect flags (clean session)
    // Client ID
    for (let i = 0; i < clientId.length; i++) {
      buffer[10 + i] = clientId.charCodeAt(i);
    }
    return buffer;
  }
  
  private processMqttPacket(data: ArrayBuffer): void {
    const view = new Uint8Array(data);
    const packetType = (view[0] & 0xF0) >> 4;
    
    switch (packetType) {
      case 2: // CONNACK
        console.log('MQTT connection acknowledged');
        this.subscribe('sensors/temperature');
        break;
      case 3: // PUBLISH
        this.handlePublish(view);
        break;
      case 9: // SUBACK
        console.log('Subscription acknowledged');
        break;
    }
  }
  
  subscribe(topic: string): void {
    // Build and send SUBSCRIBE packet
    console.log(`Subscribing to ${topic}`);
    // Implementation details omitted for brevity
  }
  
  private handlePublish(packet: Uint8Array): void {
    // Process PUBLISH packet
    // Implementation details omitted for brevity
    console.log('Received published message');
  }
}
```

## Advanced SignalR Features

### SignalR Streaming

SignalR supports streaming for continuous data flows:

```typescript
// Client-side streaming with SignalR
import * as signalR from "@microsoft/signalr";

class StreamingClient {
  private connection: signalR.HubConnection;
  
  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("/streamingHub")
      .withAutomaticReconnect()
      .build();
    
    this.connection.start().catch(err => console.error(err));
  }
  
  // Stream from server to client
  async startServerToClientStream(): Promise<void> {
    try {
      // Receive streaming data from server
      const stream = this.connection.stream("StreamSensorData", 1000, 10);
      
      // Process the stream
      stream.subscribe({
        next: (item) => {
          console.log("Received item:", item);
          this.updateChart(item);
        },
        complete: () => {
          console.log("Stream completed");
        },
        error: (err) => {
          console.error("Stream error:", err);
        }
      });
    } catch (error) {
      console.error("Error starting stream:", error);
    }
  }
  
  // Stream from client to server
  async startClientToServerStream(duration: number): Promise<void> {
    try {
      // Generate a client-side data stream
      const subject = new signalR.Subject();
      
      // Start sending stream to server
      await this.connection.send("UploadStream", subject);
      
      // Send data every 100ms
      const intervalHandle = setInterval(() => {
        const data = { 
          timestamp: new Date(), 
          value: Math.random() * 100
        };
        subject.next(data);
      }, 100);
      
      // Complete the subject after specified duration
      setTimeout(() => {
        clearInterval(intervalHandle);
        subject.complete();
      }, duration);
    } catch (error) {
      console.error("Error in client-to-server streaming:", error);
    }
  }
  
  // Bidirectional streaming with duplex connection
  async startBidirectionalStream(): Promise<void> {
    try {
      const subject = new signalR.Subject();
      
      // Start duplex stream
      const serverStream = this.connection.stream("ProcessAndRespond", subject);
      
      // Handle server responses
      serverStream.subscribe({
        next: (processedItem) => {
          console.log("Server processed data:", processedItem);
          this.updateProcessedDataDisplay(processedItem);
        },
        complete: () => console.log("Bidirectional stream completed"),
        error: (err) => console.error("Bidirectional stream error:", err)
      });
      
      // Send client data
      for (let i = 0; i < 100; i++) {
        subject.next({ id: i, data: `Data point ${i}` });
        await new Promise(resolve => setTimeout(resolve, 100));
      }
      
      subject.complete();
    } catch (error) {
      console.error("Error in bidirectional streaming:", error);
    }
  }
  
  private updateChart(data: any): void {
    // Update visualization with streaming data
    console.log("Chart updated with new data point");
  }
  
  private updateProcessedDataDisplay(data: any): void {
    // Update UI with processed data from server
    console.log("Processed data display updated");
  }
}
```

### C# Server-Side Implementation for Streaming:

```csharp
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class StreamingHub : Hub
{
    private readonly IDataService _dataService;
    
    public StreamingHub(IDataService dataService)
    {
        _dataService = dataService;
    }
    
    // Server-to-client streaming
    public async IAsyncEnumerable<SensorReading> StreamSensorData(
        int intervalMs, 
        int count,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < count && !cancellationToken.IsCancellationRequested; i++)
        {
            // Get real sensor data or generate simulated data
            var reading = await _dataService.GetLatestSensorReadingAsync();
            
            // Return the reading as part of the stream
            yield return reading;
            
            // Wait before sending next item
            await Task.Delay(intervalMs, cancellationToken);
        }
    }
    
    // Client-to-server streaming
    public async Task UploadStream(IAsyncEnumerable<ClientDataPoint> stream)
    {
        // Process incoming stream from client
        await foreach (var dataPoint in stream)
        {
            // Process and store each data point
            await _dataService.ProcessClientDataAsync(dataPoint);
            
            // Log data received
            Console.WriteLine($"Received data: timestamp={dataPoint.Timestamp}, value={dataPoint.Value}");
        }
        
        // Stream is complete
        Console.WriteLine("Client stream completed");
    }
    
    // Bidirectional streaming (duplex)
    public async IAsyncEnumerable<ProcessedData> ProcessAndRespond(
        IAsyncEnumerable<ClientDataPoint> clientStream,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Process each item from client stream
        await foreach (var dataPoint in clientStream.WithCancellation(cancellationToken))
        {
            // Process the data point
            var processed = await _dataService.ProcessDataPointAsync(dataPoint);
            
            // Return processed result to client
            yield return processed;
        }
    }
}

public class SensorReading
{
    public DateTime Timestamp { get; set; }
    public string SensorId { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; }
}

public class ClientDataPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
}

public class ProcessedData
{
    public int Id { get; set; }
    public string Result { get; set; }
    public double ProcessedValue { get; set; }
    public DateTime ProcessedAt { get; set; }
}
```

## WebRTC for Peer-to-Peer Media Streaming

One of the most powerful applications of WebRTC is peer-to-peer video conferencing:

```typescript
// WebRTC Video Chat Implementation
class VideoChatClient {
  private localStream: MediaStream | null = null;
  private peerConnection: RTCPeerConnection | null = null;
  private signaling: SignalingService;
  private remoteVideo: HTMLVideoElement;
  private localVideo: HTMLVideoElement;
  
  constructor(signalingUrl: string, localVideoId: string, remoteVideoId: string) {
    this.signaling = new SignalingService(signalingUrl);
    this.remoteVideo = document.getElementById(remoteVideoId) as HTMLVideoElement;
    this.localVideo = document.getElementById(localVideoId) as HTMLVideoElement;
    
    this.setupSignaling();
  }
  
  private setupSignaling(): void {
    this.signaling.onMessage = async (message) => {
      const { type, data, sender } = message;
      
      switch(type) {
        case 'offer':
          await this.handleOffer(data, sender);
          break;
        case 'answer':
          await this.handleAnswer(data);
          break;
        case 'ice-candidate':
          await this.handleIceCandidate(data);
          break;
        case 'user-joined':
          await this.initiateCall(sender);
          break;
        case 'user-left':
          this.handlePeerDisconnect();
          break;
      }
    };
  }
  
  async startLocalStream(): Promise<void> {
    try {
      // Request access to camera and microphone
      this.localStream = await navigator.mediaDevices.getUserMedia({
        video: { width: 1280, height: 720 },
        audio: true
      });
      
      // Display local video stream
      this.localVideo.srcObject = this.localStream;
      
      // Notify server that we're ready
      this.signaling.send({ type: 'ready', data: null });
      
      console.log('Local stream started');
    } catch (error) {
      console.error('Error accessing media devices:', error);
    }
  }
  
  private async createPeerConnection(): Promise<void> {
    try {
      this.peerConnection = new RTCPeerConnection({
        iceServers: [
          { urls: 'stun:stun.l.google.com:19302' },
          { urls: 'stun:stun1.l.google.com:19302' },
          {
            urls: 'turn:turn.example.com:3478',
            username: 'username',
            credential: 'password'
          }
        ],
        iceCandidatePoolSize: 10
      });
      
      // Add local tracks to the connection
      if (this.localStream) {
        this.localStream.getTracks().forEach(track => {
          this.peerConnection!.addTrack(track, this.localStream!);
        });
      }
      
      // Handle ICE candidates
      this.peerConnection.onicecandidate = (event) => {
        if (event.candidate) {
          this.signaling.send({
            type: 'ice-candidate',
            data: event.candidate
          });
        }
      };
      
      // Handle connection state changes
      this.peerConnection.onconnectionstatechange = () => {
        console.log('Connection state:', this.peerConnection?.connectionState);
      };
      
      // Handle remote stream
      this.peerConnection.ontrack = (event) => {
        if (event.streams && event.streams[0]) {
          this.remoteVideo.srcObject = event.streams[0];
        }
      };
      
      console.log('Peer connection created');
    } catch (error) {
      console.error('Error creating peer connection:', error);
    }
  }
  
  async initiateCall(peerId: string): Promise<void> {
    try {
      await this.createPeerConnection();
      
      // Create offer
      const offer = await this.peerConnection!.createOffer({
        offerToReceiveAudio: true,
        offerToReceiveVideo: true
      });
      
      // Set local description
      await this.peerConnection!.setLocalDescription(offer);
      
      // Send offer to peer
      this.signaling.send({
        type: 'offer',
        data: offer,
        target: peerId
      });
      
      console.log('Call initiated with offer');
    } catch (error) {
      console.error('Error initiating call:', error);
    }
  }
  
  async handleOffer(offer: RTCSessionDescriptionInit, peerId: string): Promise<void> {
    try {
      await this.createPeerConnection();
      
      // Set remote description
      await this.peerConnection!.setRemoteDescription(new RTCSessionDescription(offer));
      
      // Create answer
      const answer = await this.peerConnection!.createAnswer();
      
      // Set local description
      await this.peerConnection!.setLocalDescription(answer);
      
      // Send answer to peer
      this.signaling.send({
        type: 'answer',
        data: answer,
        target: peerId
      });
      
      console.log('Handled offer with answer');
    } catch (error) {
      console.error('Error handling offer:', error);
    }
  }
  
  async handleAnswer(answer: RTCSessionDescriptionInit): Promise<void> {
    try {
      await this.peerConnection!.setRemoteDescription(new RTCSessionDescription(answer));
      console.log('Handled answer');
    } catch (error) {
      console.error('Error handling answer:', error);
    }
  }
  
  async handleIceCandidate(candidate: RTCIceCandidateInit): Promise<void> {
    try {
      if (this.peerConnection) {
        await this.peerConnection.addIceCandidate(new RTCIceCandidate(candidate));
      }
    } catch (error) {
      console.error('Error handling ICE candidate:', error);
    }
  }
  
  handlePeerDisconnect(): void {
    console.log('Peer disconnected');
    this.remoteVideo.srcObject = null;
    
    if (this.peerConnection) {
      this.peerConnection.close();
      this.peerConnection = null;
    }
  }
  
  endCall(): void {
    this.handlePeerDisconnect();
    
    // Notify server
    this.signaling.send({ type: 'end-call', data: null });
    
    // Stop local stream
    if (this.localStream) {
      this.localStream.getTracks().forEach(track => track.stop());
      this.localStream = null;
      this.localVideo.srcObject = null;
    }
    
    console.log('Call ended');
  }
}
```

## Server-Side Event Propagation and Load Balancing

### Redis as a SignalR Backplane

For real-time applications at scale, a backplane like Redis is essential:

```csharp
// In Program.cs
var redisConnectionString = builder.Configuration["Redis:ConnectionString"];

builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnectionString, options =>
    {
        options.Configuration.ChannelPrefix = "app_name_";
        options.Configuration.DefaultDatabase = 5;
        
        // Advanced configuration
        options.ConnectionFactory = async writer =>
        {
            var connection = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
            connection.ConnectionFailed += (_, e) =>
            {
                Console.WriteLine($"Connection to Redis failed: {e.Exception}");
            };
            
            if (connection.IsConnected)
            {
                Console.WriteLine("Connected to Redis backplane");
            }
            else
            {
                Console.WriteLine("Failed to connect to Redis backplane");
            }
            
            return connection;
        };
    });
```

### Scale-Out Architecture Diagram for SignalR

Here's how a scaled-out SignalR architecture would look:

```
┌───────────────┐     ┌───────────────┐     ┌───────────────┐
│  Web Server 1 │     │  Web Server 2 │     │  Web Server 3 │
│   (SignalR)   │     │   (SignalR)   │     │   (SignalR)   │
└───────┬───────┘     └───────┬───────┘     └───────┬───────┘
        │                     │                     │
        ▼                     ▼                     ▼
┌─────────────────────────────────────────────────────────┐
│                     Load Balancer                       │
└─────────────────────────┬───────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                Redis Backplane                          │
└─────────────────────────────────────────────────────────┘
```

## Real-Time Data Visualization

### WebSockets for Live Charts

```typescript
// Live Charting with WebSockets and Chart.js
import Chart from 'chart.js/auto';

class LiveChartManager {
  private charts: Map<string, Chart> = new Map();
  private socket: WebSocket | null = null;
  private dataBuffers: Map<string, any[]> = new Map();
  private maxDataPoints: number = 100;
  
  constructor(wsUrl: string) {
    this.socket = new WebSocket(wsUrl);
    
    this.socket.onopen = () => {
      console.log('WebSocket connection established for charting');
      this.subscribeToDataFeeds();
    };
    
    this.socket.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        this.processChartData(data);
      } catch (error) {
        console.error('Error processing chart data:', error);
      }
    };
    
    this.socket.onerror = (error) => {
      console.error('WebSocket error:', error);
    };
    
    this.socket.onclose = () => {
      console.log('WebSocket connection closed');
      // Attempt to reconnect after delay
      setTimeout(() => this.reconnect(), 5000);
    };
    
    // Create initial placeholder charts
    this.initializeCharts();
  }
  
  private subscribeToDataFeeds(): void {
    if (!this.socket || this.socket.readyState !== WebSocket.OPEN) return;
    
    // Subscribe to different data streams
    this.socket.send(JSON.stringify({
      action: 'subscribe',
      feeds: [
        'cpu_usage', 
        'memory_usage', 
        'network_throughput', 
        'disk_io'
      ]
    }));
  }
  
  private initializeCharts(): void {
    // CPU Usage Chart
    this.createLineChart('cpu_chart', 'CPU Usage (%)', {
      borderColor: 'rgb(75, 192, 192)',
      tension: 0.1
    });
    
    // Memory Usage Chart
    this.createLineChart('memory_chart', 'Memory Usage (MB)', {
      borderColor: 'rgb(153, 102, 255)',
      tension: 0.1
    });
    
    // Network Throughput Chart
    this.createLineChart('network_chart', 'Network (Mbps)', {
      borderColor: 'rgb(255, 99, 132)',
      tension: 0.1
    });
    
    // Disk I/O Chart
    this.createLineChart('disk_chart', 'Disk I/O (MB/s)', {
      borderColor: 'rgb(255, 159, 64)',
      tension: 0.1
    });
    
    // Initialize data buffers for each chart
    this.dataBuffers.set('cpu_usage', []);
    this.dataBuffers.set('memory_usage', []);
    this.dataBuffers.set('network_throughput', []);
    this.dataBuffers.set('disk_io', []);
  }
  
  private createLineChart(canvasId: string, label: string, options: any): void {
    const ctx = document.getElementById(canvasId) as HTMLCanvasElement;
    
    if (!ctx) {
      console.error(`Canvas element with ID ${canvasId} not found`);
      return;
    }
    
    const chart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: [],
        datasets: [{
          label,
          data: [],
          borderColor: options.borderColor,
          tension: options.tension,
          pointRadius: 0,
          borderWidth: 1.5
        }]
      },
      options: {
        animation: {
          duration: 0 // Disable animations for better performance
        },
        scales: {
          x: {
            type: 'time',
            time: {
              unit: 'second',
              tooltipFormat: 'HH:mm:ss'
            },
            title: {
              display: true,
              text: 'Time'
            }
          },
          y: {
            beginAtZero: true
          }
        },
        plugins: {
          legend: {
            display: true
          },
          tooltip: {
            enabled: true
          }
        },
        interaction: {
          intersect: false,
          mode: 'index'
        },
        responsive: true,
        maintainAspectRatio: false
      }
    });
    
    this.charts.set(canvasId, chart);
  }
  
  private processChartData(data: any): void {
    const { type, timestamp, value } = data;
    
    // Map data types to chart IDs
    const chartMapping: {[key: string]: string} = {
      'cpu_usage': 'cpu_chart',
      'memory_usage': 'memory_chart',
      'network_throughput': 'network_chart',
      'disk_io': 'disk_chart'
    };
    
    const chartId = chartMapping[type];
    
    if (!chartId) return;
    
    // Update data buffer
    const buffer = this.dataBuffers.get(type) || [];
    buffer.push({
      x: new Date(timestamp),
      y: value
    });
    
    // Trim buffer if too large
    if (buffer.length > this.maxDataPoints) {
      buffer.shift();
    }
    
    this.dataBuffers.set(type, buffer);
    
    // Update chart
    const chart = this.charts.get(chartId);
    if (chart) {
      chart.data.datasets[0].data = [...buffer];
      chart.update('quiet'); // 'quiet' mode for better performance
    }
  }
  
  private reconnect(): void {
    try {
      if (this.socket && this.socket.readyState === WebSocket.CLOSED) {
        this.socket = new WebSocket(this.socket.url);
        // Re-establish event handlers
        // ...
      }
    } catch (error) {
      console.error('WebSocket reconnection failed:', error);
    }
  }
  
  disconnect(): void {
    if (this.socket) {
      this.socket.close();
      this.socket = null;
    }
  }
}
```

## Advanced Patterns: Circuit Breakers and Fallbacks

### Circuit Breaker Pattern for Real-Time Connections

```typescript
// Circuit Breaker for real-time connections
class ConnectionCircuitBreaker {
  private state: 'CLOSED' | 'OPEN' | 'HALF_OPEN' = 'CLOSED';
  private failureCount: number = 0;
  private lastFailureTime: number = 0;
  private resetTimeout: number;
  private failureThreshold: number;
  private connection: any; // WebSocket, SignalR, etc.
  private connectionFactory: () => Promise<any>;
  
  constructor(
    connectionFactory: () => Promise<any>,
    failureThreshold: number = 3,
    resetTimeout: number = 10000
  ) {
    this.connectionFactory = connectionFactory;
    this.failureThreshold = failureThreshold;
    this.resetTimeout = resetTimeout;
  }
  
  async connect(): Promise<any> {
    switch (this.state) {
      case 'CLOSED':
        return this.attemptConnection();
        
      case 'OPEN':
        // Check if timeout has elapsed and we should try again
        if (Date.now() - this.lastFailureTime > this.resetTimeout) {
          console.log('Circuit breaker: trying half-open state');
          this.state = 'HALF_OPEN';
          return this.attemptConnection();
        } else {
          throw new Error('Circuit is open, connection not allowed');
        }
        
      case 'HALF_OPEN':
        return this.attemptConnection();
        
      default:
        throw new Error('Unknown circuit breaker state');
    }
  }
  
  private async attemptConnection(): Promise<any> {
    try {
      this.connection = await this.connectionFactory();
      
      // If connection successful in HALF_OPEN state, reset circuit breaker
      if (this.state === 'HALF_OPEN') {
        console.log('Connection successful, closing circuit');
        this.reset();
      }
      
      return this.connection;
    } catch (error) {
      this.recordFailure();
      throw error;
    }
  }
  
  private recordFailure(): void {
    this.failureCount++;
    this.lastFailureTime = Date.now();
    
    if (this.failureCount >= this.failureThreshold || this.state === 'HALF_OPEN') {
      console.log(`Circuit breaker opening after ${this.failureCount} failures`);
      this.state = 'OPEN';
    }
  }
  
  reset(): void {
    this.state = 'CLOSED';
    this.failureCount = 0;
  }
  
  getState(): string {
    return this.state;
  }
}

// Usage
const circuitBreaker = new ConnectionCircuitBreaker(
  async () => {
    const connection = new WebSocket('wss://api.example.com/realtime');
    
    // Wait for connection to establish or fail
    return new Promise((resolve, reject) => {
      connection.onopen = () => resolve(connection);
      connection.onerror = (error) => reject(error);
      
      // Timeout after 5 seconds
      setTimeout(() => reject(new Error('Connection timeout')), 5000);
    });
  },
  3,    // Failure threshold
  10000 // Reset timeout in ms
);

// Try to connect
try {
  const connection = await circuitBreaker.connect();
  // Use connection...
} catch (error) {
  console.error('Connection failed:', error);
  // Fall back to alternative method...
}
```

### Fallback Strategy with Progressive Enhancement

```typescript
class RealTimeConnectionManager {
  private activeConnection: any = null;
  private connectionType: 'websocket' | 'sse' | 'long-polling' | 'polling' | 'none' = 'none';
  private connectionOptions: {
    baseUrl: string;
    path: string;
    authToken?: string;
  };
  
  constructor(options: {
    baseUrl: string;
    path: string;
    authToken?: string;
  }) {
    this.connectionOptions = options;
  }
  
  async connect(): Promise<boolean> {
    // Try connection methods in order of preference
    return (
      await this.tryWebSocket() ||
      await this.tryServerSentEvents() ||
      await this.tryLongPolling() ||
      await this.tryPolling()
    );
  }
  
  private async tryWebSocket(): Promise<boolean> {
    try {
      if (!window.WebSocket) return false;
      
      const socket = new WebSocket(
        `${this.connectionOptions.baseUrl.replace('http', 'ws')}/${this.connectionOptions.path}`
      );
      
      await new Promise<void>((resolve, reject) => {
        socket.onopen = () => resolve();
        socket.onerror = (error) => reject(error);
        setTimeout(() => reject(new Error('WebSocket connection timeout')), 5000);
      });
      
      this.configureWebSocket(socket);
      this.activeConnection = socket;
      this.connectionType = 'websocket';
      console.log('Using WebSocket connection');
      return true;
    } catch (error) {
      console.warn('WebSocket connection failed, falling back:', error);
      return false;
    }
  }
  
  private async tryServerSentEvents(): Promise<boolean> {
    try {
      if (!window.EventSource) return false;
      
      const url = `${this.connectionOptions.baseUrl}/${this.connectionOptions.path}/sse`;
      const eventSource = new EventSource(url, {
        withCredentials: true
      });
      
      await new Promise<void>((resolve, reject) => {
        eventSource.onopen = () => resolve();
        eventSource.onerror = (error) => reject(error);
        setTimeout(() => reject(new Error('SSE connection timeout')), 5000);
      });
      
      this.configureServerSentEvents(eventSource);
      this.activeConnection = eventSource;
      this.connectionType = 'sse';
      console.log('Using Server-Sent Events connection');
      return true;
    } catch (error) {
      console.warn('Server-Sent Events connection failed, falling back:', error);
      return false;
    }
  }
  
  private async tryLongPolling(): Promise<boolean> {
    try {
      const url = `${this.connectionOptions.baseUrl}/${this.connectionOptions.path}/long-poll`;
      
      // Test if long polling works
      const response = await fetch(`${url}/test`, {
        method: 'GET',
        headers: {
          'Accept': 'application/json',
          'Authorization': this.connectionOptions.authToken ? 
            `Bearer ${this.connectionOptions.authToken}` : ''
        }
      });
      
      if (!response.ok) throw new Error(`Long polling test failed: ${response.status}`);
      
      // Set up long polling client
      const longPollClient = {
        url,
        isPolling: true,
        lastMessageId: null,
        poll: async () => {
          // Implementation details omitted for brevity
        },
        disconnect: () => {
          longPollClient.isPolling = false;
        }
      };
      
      this.activeConnection = longPollClient;
      this.connectionType = 'long-polling';
      longPollClient.poll(); // Start polling
      console.log('Using long polling connection');
      return true;
    } catch (error) {
      console.warn('Long polling connection failed, falling back:', error);
      return false;
    }
  }
  
  private async tryPolling(): Promise<boolean> {
    try {
      const url = `${this.connectionOptions.baseUrl}/${this.connectionOptions.path}/poll`;
      
      const pollingClient = {
        url,
        isPolling: true,
        pollInterval: 3000, // Poll every 3 seconds
        intervalId: null as number | null,
        poll: async () => {
          // Implementation details omitted for brevity
          
          // Start interval for regular polling
          pollingClient.intervalId = window.setInterval(async () => {
            // Poll implementation
          }, pollingClient.pollInterval);
        },
        disconnect: () => {
          if (pollingClient.intervalId !== null) {
            window.clearInterval(pollingClient.intervalId);
          }
          pollingClient.isPolling = false;
        }
      };
      
      this.activeConnection = pollingClient;
      this.connectionType = 'polling';
      pollingClient.poll(); // Start polling
      console.log('Using regular polling connection');
      return true;
    } catch (error) {
      console.warn('All connection methods failed');
      return false;
    }
  }
  
  private configureWebSocket(socket: WebSocket): void {
    // Configure WebSocket specific handlers
    socket.onmessage = (event) => {
      this.handleIncomingMessage(JSON.parse(event.data));
    };
  }
  
  private configureServerSentEvents(eventSource: EventSource): void {
    // Configure SSE specific handlers
    eventSource.addEventListener('message', (event) => {
      this.handleIncomingMessage(JSON.parse(event.data));
    });
  }
  
  private handleIncomingMessage(data: any): void {
    // Process incoming messages
    console.log('Message received:', data);
    // Dispatch to appropriate handlers based on message type
  }
  
  sendMessage(message: any): boolean {
    switch (this.connectionType) {
      case 'websocket':
        (this.activeConnection as WebSocket).send(JSON.stringify(message));
        return true;
        
      case 'sse':
        // SSE is one-way, so use a separate HTTP request
        fetch(`${this.connectionOptions.baseUrl}/${this.connectionOptions.path}/message`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': this.connectionOptions.authToken ? 
              `Bearer ${this.connectionOptions.authToken}` : ''
          },
          body: JSON.stringify(message)
        }).catch(error => console.error('Error sending message:', error));
        return true;
        
      case 'long-polling':
      case 'polling':
        fetch(`${this.connectionOptions.baseUrl}/${this.connectionOptions.path}/message`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': this.connectionOptions.authToken ? 
              `Bearer ${this.connectionOptions.authToken}` : ''
          },
          body: JSON.stringify(message)
        }).catch(error => console.error('Error sending message:', error));
        return true;
        
      case 'none':
      default:
        console.error('No active connection');
        return false;
    }
  }
  
  disconnect(): void {
    if (!this.activeConnection) return;
    
    switch (this.connectionType) {
      case 'websocket':
        (this.activeConnection as WebSocket).close();
        break;
        
      case 'sse':
        (this.activeConnection as EventSource).close();
        break;
        
      case 'long-polling':
      case 'polling':
        this.activeConnection.disconnect();
        break;
    }
    
    this.activeConnection = null;
    this.connectionType = 'none';
    console.log('Connection closed');
  }
  
  getConnectionType(): string {
    return this.connectionType;
  }
}
```

## Performance Benchmarking of Real-Time Protocols

In a real-time application lecture, it's valuable to compare the performance characteristics of different protocols:

| Protocol | Latency | Overhead | Scalability | Client Support | Bi-directional | Firewall Friendly |
|----------|---------|----------|-------------|---------------|----------------|------------------|
| WebSocket | Very Low (30-50ms) | Low | Medium | Excellent | Yes | Medium |
| SignalR | Low (50-100ms) | Medium | High | Excellent | Yes | High |
| SSE | Medium (100-200ms) | Low | High | Good | No | High |
| Long Polling | High (300+ ms) | High | Low | Excellent | Simulated | High |
| Short Polling | Very High (depends on interval) | Very High | Very Low | Excellent | Simulated | High |
| WebRTC | Lowest (<30ms) | Medium | Low (P2P) | Good | Yes | Low |
| MQTT | Very Low (30-50ms) | Very Low | Very High | Limited | Yes | Medium |

## Security Deep Dive

### WebSocket Authentication with JWT

```typescript
// WebSocket with JWT authentication
class SecureWebSocketClient {
  private socket: WebSocket | null = null;
  private authToken: string;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  
  constructor(private url: string, authToken: string) {
    this.authToken = authToken;
  }
  
  connect(): void {
    try {
      // Add JWT token as query parameter
      const secureUrl = `${this.url}?token=${encodeURIComponent(this.authToken)}`;
      this.socket = new WebSocket(secureUrl);
      
      this.socket.onopen = () => {
        console.log('Secure WebSocket connection established');
        this.reconnectAttempts = 0;
      };
      
      this.socket.onclose = (event) => {
        if (event.code === 1008) {
          // Authentication failure
          console.error('Authentication failed');
        } else {
          this.handleReconnect();
        }
      };
    } catch (error) {
      console.error('Connection error:', error);
      this.handleReconnect();
    }
  }
  
  // Additional methods for handling reconnection and messaging
}

// C# Server Implementation for JWT validation
[WebSocketController]
public class SecureWebSocketHandler
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly TokenValidationParameters _tokenValidation;
    
    public SecureWebSocketHandler(IConfiguration config)
    {
        _tokenHandler = new JwtSecurityTokenHandler();
        _tokenValidation = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config["Jwt:Key"])),
            ValidateIssuer = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = config["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
    
    public async Task<bool> AuthenticateConnection(HttpContext context, WebSocket webSocket)
    {
        try
        {
            string token = context.Request.Query["token"];
            
            if (string.IsNullOrEmpty(token))
            {
                await CloseSocketWithError(webSocket, "Missing authentication token");
                return false;
            }
            
            var principal = _tokenHandler.ValidateToken(token, _tokenValidation, out var validatedToken);
            
            // Store user info in the context for later use
            context.Items["User"] = principal;
            
            return true;
        }
        catch (SecurityTokenException ex)
        {
            await CloseSocketWithError(webSocket, "Invalid token: " + ex.Message);
            return false;
        }
    }
    
    private async Task CloseSocketWithError(WebSocket socket, string message)
    {
        if (socket.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await socket.CloseAsync(
                WebSocketCloseStatus.PolicyViolation,
                message,
                CancellationToken.None);
        }
    }
}
```

## Cross-Platform Considerations

### Working with Web Workers for Background Processing

```typescript
// Main thread
class WorkerBasedRealTimeClient {
  private worker: Worker;
  private connectionReady = false;
  private messageQueue: any[] = [];
  
  constructor() {
    // Create Web Worker for handling WebSocket communication
    this.worker = new Worker('websocket-worker.js');
    
    this.worker.onmessage = (event) => {
      const { type, data } = event.data;
      
      switch (type) {
        case 'connection-established':
          this.connectionReady = true;
          this.flushQueue();
          break;
          
        case 'connection-lost':
          this.connectionReady = false;
          break;
          
        case 'message':
          this.handleIncomingMessage(data);
          break;
          
        case 'error':
          console.error('WebSocket worker error:', data);
          break;
      }
    };
    
    // Initialize WebSocket connection in worker
    this.worker.postMessage({
      type: 'connect',
      url: 'wss://api.example.com/realtime'
    });
  }
  
  sendMessage(message: any): void {
    if (this.connectionReady) {
      this.worker.postMessage({
        type: 'send',
        data: message
      });
    } else {
      // Queue message to send when connection is ready
      this.messageQueue.push(message);
    }
  }
  
  private flushQueue(): void {
    while (this.messageQueue.length > 0 && this.connectionReady) {
      const message = this.messageQueue.shift();
      this.sendMessage(message);
    }
  }
  
  private handleIncomingMessage(data: any): void {
    // Process incoming real-time data
    console.log('Received real-time data:', data);
    
    // Update UI or trigger other actions
    // This runs on the main thread
  }
  
  disconnect(): void {
    this.worker.postMessage({ type: 'disconnect' });
  }
}

// websocket-worker.js - Web Worker
self.addEventListener('message', function(e) {
  const { type, url, data } = e.data;
  
  switch (type) {
    case 'connect':
      connectWebSocket(url);
      break;
      
    case 'send':
      sendMessage(data);
      break;
      
    case 'disconnect':
      closeConnection();
      break;
  }
});

let socket = null;

function connectWebSocket(url) {
  try {
    socket = new WebSocket(url);
    
    socket.onopen = function() {
      self.postMessage({ type: 'connection-established' });
    };
    
    socket.onmessage = function(event) {
      try {
        const data = JSON.parse(event.data);
        self.postMessage({ type: 'message', data });
      } catch (error) {
        self.postMessage({ 
          type: 'error', 
          data: 'Failed to parse message: ' + error.message 
        });
      }
    };
    
    socket.onerror = function(error) {
      self.postMessage({ type: 'error', data: 'Socket error' });
    };
    
    socket.onclose = function() {
      self.postMessage({ type: 'connection-lost' });
    };
  } catch (error) {
    self.postMessage({ 
      type: 'error', 
      data: 'Failed to create WebSocket: ' + error.message 
    });
  }
}

function sendMessage(data) {
  if (socket && socket.readyState === WebSocket.OPEN) {
    socket.send(JSON.stringify(data));
  } else {
    self.postMessage({ 
      type: 'error', 
      data: 'Cannot send message, socket not open' 
    });
  }
}

function closeConnection() {
  if (socket) {
    socket.close();
    socket = null;
  }
}
```

## QoS (Quality of Service) Implementations

### MQTT-style QoS in Web Applications

```typescript
// Implementing QoS levels in web messaging
class QoSMessenger {
  private messageStore = new Map<string, QoSMessage>();
  private socket: WebSocket;
  private pendingAcks = new Map<string, NodeJS.Timeout>();
  private qos: 0 | 1 | 2 = 1; // Default to QoS 1
  private ackTimeoutMs = 5000;
  private maxRetries = 3;
  
  constructor(url: string, qos: 0 | 1 | 2 = 1) {
    this.socket = new WebSocket(url);
    this.qos = qos;
    
    this.setupSocketHandlers();
  }
  
  private setupSocketHandlers(): void {
    this.socket.onmessage = (event) => {
      try {
        const message = JSON.parse(event.data);
        
        if (message.type === 'ack') {
          this.handleAck(message.id);
        } else if (message.type === 'message') {
          this.handleIncomingMessage(message);
        }
      } catch (error) {
        console.error('Error processing message:', error);
      }
    };
  }
  
  sendMessage(data: any, qos?: 0 | 1 | 2): string {
    const messageQoS = qos !== undefined ? qos : this.qos;
    const messageId = this.generateMessageId();
    
    const message: QoSMessage = {
      id: messageId,
      data,
      qos: messageQoS,
      sentTime: Date.now(),
      attempts: 1
    };
    
    // For QoS 1 or 2, store the message for potential retransmission
    if (messageQoS > 0) {
      this.messageStore.set(messageId, message);
    }
    
    this.transmitMessage(message);
    return messageId;
  }
  
  private transmitMessage(message: QoSMessage): void {
    if (this.socket.readyState !== WebSocket.OPEN) {
      console.warn('Socket not open, queuing message');
      // Could implement queue here
      return;
    }
    
    const outgoingMessage = {
      id: message.id,
      type: 'message',
      qos: message.qos,
      data: message.data
    };
    
    this.socket.send(JSON.stringify(outgoingMessage));
    
    // For QoS 1 or 2, set up an ack timeout
    if (message.qos > 0) {
      this.setupAckTimeout(message.id);
    }
  }
  
  private handleAck(messageId: string): void {
    // Clear the ack timeout
    if (this.pendingAcks.has(messageId)) {
      clearTimeout(this.pendingAcks.get(messageId)!);
      this.pendingAcks.delete(messageId);
    }
    
    // For QoS 1, we can remove the message from store now
    if (this.messageStore.has(messageId)) {
      const message = this.messageStore.get(messageId)!;
      
      if (message.qos === 1) {
        this.messageStore.delete(messageId);
      }
      // For QoS 2, additional handling would be needed here
    }
  }
  
  private setupAckTimeout(messageId: string): void {
    // Clear any existing timeout
    if (this.pendingAcks.has(messageId)) {
      clearTimeout(this.pendingAcks.get(messageId)!);
    }
    
    // Set new timeout
    const timeoutId = setTimeout(() => {
      this.handleAckTimeout(messageId);
    }, this.ackTimeoutMs);
    
    this.pendingAcks.set(messageId, timeoutId);
  }
  
  private handleAckTimeout(messageId: string): void {
    if (!this.messageStore.has(messageId)) return;
    
    const message = this.messageStore.get(messageId)!;
    
    if (message.attempts >= this.maxRetries) {
      // Maximum retries reached, give up
      console.error(`Message ${messageId} failed after ${message.attempts} attempts`);
      this.messageStore.delete(messageId);
      this.pendingAcks.delete(messageId);
      
      // Could notify application layer of message failure
      return;
    }
    
    // Increment attempt counter and resend
    message.attempts++;
    this.messageStore.set(messageId, message);
    console.log(`Retrying message ${messageId}, attempt ${message.attempts}`);
    this.transmitMessage(message);
  }
  
  private handleIncomingMessage(message: any): void {
    // Process incoming message
    console.log('Received message:', message);
    
    // For QoS 1 or 2, send acknowledgement
    if (message.qos > 0) {
      this.sendAck(message.id);
    }
    
    // Process message data (application specific)
    // ...
  }
  
  private sendAck(messageId: string): void {
    const ack = {
      type: 'ack',
      id: messageId
    };
    
    this.socket.send(JSON.stringify(ack));
  }
  
  private generateMessageId(): string {
    // Generate unique message ID
    return Date.now().toString(36) + Math.random().toString(36).substr(2, 5);
  }
}

interface QoSMessage {
  id: string;
  data: any;
  qos: 0 | 1 | 2;
  sentTime: number;
  attempts: number;
}
```

## Real-Time Analytics Use Case

```typescript
// Real-Time Analytics Dashboard Client
class AnalyticsDashboard {
  private socket: WebSocket;
  private charts: Map<string, any> = new Map();
  private metrics: {
    activeUsers: number;
    pageViews: number;
    conversionRate: number;
    errorRate: number;
    responseTime: number;
  } = {
    activeUsers: 0,
    pageViews: 0,
    conversionRate: 0,
    errorRate: 0,
    responseTime: 0
  };
  
  // Time-based data storage
  private timeSeriesData: Map<string, Array<{time: Date, value: number}>> = new Map([
    ['activeUsers', []],
    ['pageViews', []],
    ['conversionRate', []],
    ['errorRate', []],
    ['responseTime', []]
  ]);
  
  constructor(socketUrl: string) {
    this.socket = new WebSocket(socketUrl);
    
    this.socket.onopen = () => {
      console.log('Analytics socket connected');
      this.subscribe();
    };
    
    this.socket.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        this.processAnalyticsUpdate(data);
      } catch (error) {
        console.error('Error processing analytics update:', error);
      }
    };
    
    this.socket.onerror = (error) => {
      console.error('Analytics socket error:', error);
    };
    
    this.socket.onclose = () => {
      console.log('Analytics socket closed, reconnecting in 5s...');
      setTimeout(() => this.reconnect(), 5000);
    };
    
    // Initialize charts
    this.initializeCharts();
    
    // Update charts every second
    setInterval(() => this.updateCharts(), 1000);
  }
  
  private subscribe(): void {
    // Subscribe to analytics channels
    this.socket.send(JSON.stringify({
      action: 'subscribe',
      channels: [
        'user-activity',
        'performance-metrics',
        'error-tracking',
        'conversion-data'
      ]
    }));
  }
  
  private processAnalyticsUpdate(data: any): void {
    const now = new Date();
    
    // Update current metrics
    if (data.activeUsers !== undefined) {
      this.metrics.activeUsers = data.activeUsers;
      this.addTimeSeriesDataPoint('activeUsers', data.activeUsers, now);
    }
    
    if (data.pageViews !== undefined) {
      this.metrics.pageViews = data.pageViews;
      this.addTimeSeriesDataPoint('pageViews', data.pageViews, now);
    }
    
    if (data.conversionRate !== undefined) {
      this.metrics.conversionRate = data.conversionRate;
      this.addTimeSeriesDataPoint('conversionRate', data.conversionRate, now);
    }
    
    if (data.errorRate !== undefined) {
      this.metrics.errorRate = data.errorRate;
      this.addTimeSeriesDataPoint('errorRate', data.errorRate, now);
    }
    
    if (data.responseTime !== undefined) {
      this.metrics.responseTime = data.responseTime;
      this.addTimeSeriesDataPoint('responseTime', data.responseTime, now);
    }
    
    // Update dashboard number displays
    this.updateMetricDisplays();
  }
  
  private addTimeSeriesDataPoint(metric: string, value: number, time: Date): void {
    const series = this.timeSeriesData.get(metric);
    
    if (series) {
      series.push({ time, value });
      
      // Keep only last 60 data points (1 minute at 1 data point per second)
      if (series.length > 60) {
        series.shift();
      }
    }
  }
  
  private initializeCharts(): void {
    // Initialize charts for each metric
    // Implementation would depend on charting library
    console.log('Initializing analytics charts');
  }
  
  private updateCharts(): void {
    // Update all charts with latest data
    for (const [metric, series] of this.timeSeriesData.entries()) {
      const chart = this.charts.get(metric);
      if (chart) {
        // Update chart with series data
        // Implementation would depend on charting library
      }
    }
  }
  
  private updateMetricDisplays(): void {
    // Update real-time metric displays in the UI
    document.getElementById('active-users')!.textContent = 
      this.metrics.activeUsers.toString();
      
    document.getElementById('page-views')!.textContent = 
      this.metrics.pageViews.toString();
      
    document.getElementById('conversion-rate')!.textContent = 
      `${this.metrics.conversionRate.toFixed(2)}%`;
      
    document.getElementById('error-rate')!.textContent = 
      `${this.metrics.errorRate.toFixed(2)}%`;
      
    document.getElementById('response-time')!.textContent = 
      `${this.metrics.responseTime.toFixed(0)}ms`;
  }
  
  private reconnect(): void {
    if (this.socket.readyState === WebSocket.CLOSED) {
      this.socket = new WebSocket(this.socket.url);
      // Reattach event handlers
      // ...
    }
  }
}
```

## Comparing Implementations Across Mobile and Web Platforms

A key challenge in real-time applications is maintaining consistent behavior across platforms. This table compares support for various real-time technologies:

| Platform | WebSockets | SSE | SignalR | WebRTC | MQTT | HTTP/2 Push |
|----------|-----------|-----|---------|--------|------|------------|
| Chrome | ✅ | ✅ | ✅ | ✅ | Via Libraries | ✅ |
| Firefox | ✅ | ✅ | ✅ | ✅ | Via Libraries | ✅ |
| Safari | ✅ | ✅ | ✅ | ✅ | Via Libraries | ✅ |
| Edge | ✅ | ✅ | ✅ | ✅ | Via Libraries | ✅ |
| iOS | ✅ | ✅ | ✅ | Limited | Via Libraries | ✅ |
| Android | ✅ | ✅ | ✅ | ✅ | Via Libraries | ✅ |
| React Native | Via Libraries | Via Libraries | Via Libraries | Limited | Via Libraries | Via Libraries |
| Flutter | Via Libraries | Via Libraries | Via Libraries | Via Libraries | Via Libraries | Via Libraries |
| .NET MAUI | ✅ | ✅ | Native | Via Libraries | Via Libraries | ✅ |

This comparison helps developers choose technologies that work across their target platforms and devices.

## Conclusion

Real-time web applications represent a significant evolution in web technology, enabling instantaneous communication that was once impossible. The key takeaways for implementing successful real-time systems include:

1. **Choose the right protocol for your use case**: WebSockets for low-latency bidirectional communication, SSE for server-to-client updates, SignalR for cross-platform abstraction, WebRTC for peer-to-peer media, and MQTT for IoT scenarios.

2. **Plan for scale from the beginning**: Implement backplanes, consider message delivery patterns, and design with horizontal scaling in mind.

3. **Implement robust error handling and reconnection strategies**: Use circuit breakers, exponential backoff, and appropriate fallback mechanisms.

4. **Consider security at every layer**: Implement proper authentication, secure your connections, validate messages, and prevent common attack vectors.

5. **Optimize for performance**: Use binary protocols where appropriate, implement message batching, and consider data compression.

6. **Test thoroughly**: Test under various network conditions, simulate failures, and stress test your system to understand its limits.

By incorporating these technologies and principles, you can create responsive, resilient, and engaging real-time web applications that provide users with the immediate feedback they've come to expect from modern applications.
