# Real-Time Monitoring with WebSockets, SignalR and SSE

This example demonstrates how to implement real-time monitoring dashboards with each of the three technologies.

## 1. WebSockets Monitoring Example

### Backend Implementation (C#)

```csharp
// MonitoringWebSocketService.cs

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Http;

public class MonitoringWebSocketService
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private readonly Timer _broadcastTimer;
    private readonly Random _random = new();
    
    public MonitoringWebSocketService()
    {
        // Broadcast system metrics every 2 seconds
        _broadcastTimer = new Timer(BroadcastMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
    }
    
    // Method to handle incoming WebSocket connections
    public async Task HandleConnection(HttpContext context)
    {
        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString();
        
        // Add to active connections
        _connections.TryAdd(connectionId, socket);
        
        // Receive loop - just to keep the connection open
        var buffer = new byte[1024];
        var receiveResult = await socket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
            
        while (!receiveResult.CloseStatus.HasValue)
        {
            receiveResult = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }
        
        // Remove the connection
        _connections.TryRemove(connectionId, out _);
        
        // Close the socket
        await socket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
    
    // Method to broadcast metrics to all connections
    private async void BroadcastMetrics(object state)
    {
        try
        {
            // Generate simulated metrics
            var metrics = GenerateMetrics();
            var metricsJson = JsonSerializer.Serialize(metrics);
            var buffer = Encoding.UTF8.GetBytes(metricsJson);
            
            // List of dead connections to clean up
            var deadConnections = new List<string>();
            
            // Send to all connections
            foreach (var (id, socket) in _connections)
            {
                try
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        await socket.SendAsync(
                            new ArraySegment<byte>(buffer),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                    else
                    {
                        deadConnections.Add(id);
                    }
                }
                catch (Exception)
                {
                    deadConnections.Add(id);
                }
            }
            
            // Clean up dead connections
            foreach (var id in deadConnections)
            {
                _connections.TryRemove(id, out _);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error broadcasting metrics: {ex.Message}");
        }
    }
    
    // Generate random metrics for simulation
    private object GenerateMetrics()
    {
        return new
        {
            timestamp = DateTime.UtcNow,
            cpu = _random.Next(10, 90),
            memory = _random.Next(30, 80),
            requests = _random.Next(100, 1000),
            activeConnections = _connections.Count,
            diskUsage = _random.Next(40, 95),
            networkIn = _random.Next(100, 5000),
            networkOut = _random.Next(100, 5000)
        };
    }
}
```

### Frontend Implementation (TypeScript/React)

```tsx
// SystemMonitorWebSocket.tsx

import React, { useEffect, useState, useRef } from 'react';

interface SystemMetrics {
  timestamp: string;
  cpu: number;
  memory: number;
  requests: number;
  activeConnections: number;
  diskUsage: number;
  networkIn: number;
  networkOut: number;
}

const SystemMonitorWebSocket: React.FC = () => {
  const [metrics, setMetrics] = useState<SystemMetrics | null>(null);
  const [connected, setConnected] = useState<boolean>(false);
  const wsRef = useRef<WebSocket | null>(null);
  const chartRef = useRef<HTMLCanvasElement | null>(null);
  const cpuHistory = useRef<number[]>([]);
  const memoryHistory = useRef<number[]>([]);
  
  // Connect to WebSocket
  useEffect(() => {
    const ws = new WebSocket('ws://localhost:5000/metrics-ws');
    
    ws.onopen = () => {
      console.log('WebSocket connected');
      setConnected(true);
    };
    
    ws.onclose = () => {
      console.log('WebSocket disconnected');
      setConnected(false);
    };
    
    ws.onmessage = (event) => {
      const newMetrics: SystemMetrics = JSON.parse(event.data);
      setMetrics(newMetrics);
      
      // Update history arrays for charts
      cpuHistory.current.push(newMetrics.cpu);
      memoryHistory.current.push(newMetrics.memory);
      
      if (cpuHistory.current.length > 20) {
        cpuHistory.current.shift();
        memoryHistory.current.shift();
      }
      
      // Update chart if ref is available
      updateChart();
    };
    
    wsRef.current = ws;
    
    return () => {
      ws.close();
    };
  }, []);
  
  // Simple chart drawing function using Canvas API
  const updateChart = () => {
    const canvas = chartRef.current;
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    
    // Clear canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    // Draw CPU line (red)
    ctx.beginPath();
    ctx.strokeStyle = 'red';
    ctx.lineWidth = 2;
    
    const cpuData = cpuHistory.current;
    const width = canvas.width / (cpuData.length - 1);
    
    for (let i = 0; i < cpuData.length; i++) {
      const x = i * width;
      const y = canvas.height - (cpuData[i] / 100 * canvas.height);
      
      if (i === 0) {
        ctx.moveTo(x, y);
      } else {
        ctx.lineTo(x, y);
      }
    }
    
    ctx.stroke();
    
    // Draw Memory line (blue)
    ctx.beginPath();
    ctx.strokeStyle = 'blue';
    
    const memData = memoryHistory.current;
    
    for (let i = 0; i < memData.length; i++) {
      const x = i * width;
      const y = canvas.height - (memData[i] / 100 * canvas.height);
      
      if (i === 0) {
        ctx.moveTo(x, y);
      } else {
        ctx.lineTo(x, y);
      }
    }
    
    ctx.stroke();
  };
  
  return (
    <div className="system-monitor">
      <h2>System Monitor (WebSocket)</h2>
      
      <div className="connection-status">
        Status: {connected ? 'Connected' : 'Disconnected'}
      </div>
      
      {metrics && (
        <div className="metrics-grid">
          <div className="metric-card">
            <div className="metric-title">CPU</div>
            <div className="metric-value">{metrics.cpu}%</div>
            <div className="metric-meter">
              <div className="meter-fill" style={{ width: `${metrics.cpu}%`, backgroundColor: metrics.cpu > 80 ? 'red' : '#3498db' }}></div>
            </div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Memory</div>
            <div className="metric-value">{metrics.memory}%</div>
            <div className="metric-meter">
              <div className="meter-fill" style={{ width: `${metrics.memory}%` }}></div>
            </div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Disk Usage</div>
            <div className="metric-value">{metrics.diskUsage}%</div>
            <div className="metric-meter">
              <div className="meter-fill" style={{ width: `${metrics.diskUsage}%` }}></div>
            </div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Requests</div>
            <div className="metric-value">{metrics.requests}</div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Network In</div>
            <div className="metric-value">{metrics.networkIn} KB/s</div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Network Out</div>
            <div className="metric-value">{metrics.networkOut} KB/s</div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Active Connections</div>
            <div className="metric-value">{metrics.activeConnections}</div>
          </div>
          
          <div className="metric-card wide">
            <div className="metric-title">Last Update</div>
            <div className="metric-value">{new Date(metrics.timestamp).toLocaleTimeString()}</div>
          </div>
        </div>
      )}
      
      <div className="chart-container">
        <h3>Resource Usage History</h3>
        <div className="chart-legend">
          <div className="legend-item">
            <div className="color-box cpu"></div>
            <span>CPU</span>
          </div>
          <div className="legend-item">
            <div className="color-box memory"></div>
            <span>Memory</span>
          </div>
        </div>
        <canvas ref={chartRef} width="500" height="200"></canvas>
      </div>
    </div>
  );
};

export default SystemMonitorWebSocket;
```

## 2. SignalR Monitoring Example

### Backend Implementation (C#)

```csharp
// MonitoringHub.cs

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

public class MonitoringHub : Hub
{
    private static readonly Timer _broadcastTimer;
    private static readonly Random _random = new();
    
    static MonitoringHub()
    {
        // Start the broadcast timer when the class is first used
        _broadcastTimer = new Timer(BroadcastMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
    }
    
    // Method to count connections
    private static int ConnectionCount => _connections?.Count ?? 0;
    private static readonly ConnectionTracker _connections = new();
    
    // Override connection methods to track client connections
    public override Task OnConnectedAsync()
    {
        _connections.Add(Context.ConnectionId);
        return base.OnConnectedAsync();
    }
    
    public override Task OnDisconnectedAsync(Exception exception)
    {
        _connections.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
    
    // Send metrics to all clients
    private static async void BroadcastMetrics(object state)
    {
        try
        {
            var metrics = GenerateMetrics();
            var hubContext = GetHubContext();
            
            if (hubContext != null)
            {
                await hubContext.Clients.All.SendAsync("ReceiveMetrics", metrics);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error broadcasting metrics: {ex.Message}");
        }
    }
    
    // Helper to get the hub context
    private static IHubContext<MonitoringHub> GetHubContext()
    {
        // In a real application, use dependency injection
        // This is just for demonstration
        return (IHubContext<MonitoringHub>)
            Program.ServiceProvider?.GetService(typeof(IHubContext<MonitoringHub>));
    }
    
    // Generate random metrics
    private static object GenerateMetrics()
    {
        return new
        {
            timestamp = DateTime.UtcNow,
            cpu = _random.Next(10, 90),
            memory = _random.Next(30, 80),
            requests = _random.Next(100, 1000),
            activeConnections = ConnectionCount,
            diskUsage = _random.Next(40, 95),
            networkIn = _random.Next(100, 5000),
            networkOut = _random.Next(100, 5000)
        };
    }
    
    // Helper class to track connections
    private class ConnectionTracker
    {
        private readonly HashSet<string> _connections = new();
        private readonly object _lock = new();
        
        public void Add(string connectionId)
        {
            lock (_lock)
            {
                _connections.Add(connectionId);
            }
        }
        
        public void Remove(string connectionId)
        {
            lock (_lock)
            {
                _connections.Remove(connectionId);
            }
        }
        
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _connections.Count;
                }
            }
        }
    }
}
```

### Frontend Implementation (TypeScript/React)

```tsx
// SystemMonitorSignalR.tsx

import React, { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';

interface SystemMetrics {
  timestamp: string;
  cpu: number;
  memory: number;
  requests: number;
  activeConnections: number;
  diskUsage: number;
  networkIn: number;
  networkOut: number;
}

const SystemMonitorSignalR: React.FC = () => {
  const [metrics, setMetrics] = useState<SystemMetrics | null>(null);
  const [connectionState, setConnectionState] = useState<string>('Disconnected');
  const connection = useRef<signalR.HubConnection | null>(null);
  const chartRef = useRef<HTMLCanvasElement | null>(null);
  const cpuHistory = useRef<number[]>([]);
  const memoryHistory = useRef<number[]>([]);
  
  // Connect to SignalR hub
  useEffect(() => {
    const setupConnection = async () => {
      connection.current = new signalR.HubConnectionBuilder()
        .withUrl('/monitoringHub')
        .withAutomaticReconnect()
        .build();
      
      // Set up event handlers
      connection.current.on('ReceiveMetrics', (data: SystemMetrics) => {
        setMetrics(data);
        
        // Update history arrays
        cpuHistory.current.push(data.cpu);
        memoryHistory.current.push(data.memory);
        
        if (cpuHistory.current.length > 20) {
          cpuHistory.current.shift();
          memoryHistory.current.shift();
        }
        
        // Update chart
        updateChart();
      });
      
      // Connection state events
      connection.current.onreconnecting(() => {
        setConnectionState('Reconnecting');
      });
      
      connection.current.onreconnected(() => {
        setConnectionState('Connected');
      });
      
      connection.current.onclose(() => {
        setConnectionState('Disconnected');
      });
      
      // Start the connection
      try {
        await connection.current.start();
        setConnectionState('Connected');
      } catch (err) {
        console.error('Error connecting to SignalR hub:', err);
        setConnectionState('Error');
      }
    };
    
    setupConnection();
    
    // Cleanup
    return () => {
      connection.current?.stop();
    };
  }, []);
  
  // Chart drawing function
  const updateChart = () => {
    const canvas = chartRef.current;
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    
    // Clear canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    // Draw CPU line (red)
    ctx.beginPath();
    ctx.strokeStyle = 'red';
    ctx.lineWidth = 2;
    
    const cpuData = cpuHistory.current;
    const width = canvas.width / (cpuData.length - 1);
    
    for (let i = 0; i < cpuData.length; i++) {
      const x = i * width;
      const y = canvas.height - (cpuData[i] / 100 * canvas.height);
      
      if (i === 0) {
        ctx.moveTo(x, y);
      } else {
        ctx.lineTo(x, y);
      }
    }
    
    ctx.stroke();
    
    // Draw Memory line (blue)
    ctx.beginPath();
    ctx.strokeStyle = 'blue';
    
    const memData = memoryHistory.current;
    
    for (let i = 0; i < memData.length; i++) {
      const x = i * width;
      const y = canvas.height - (memData[i] / 100 * canvas.height);
      
      if (i === 0) {
        ctx.moveTo(x, y);
      } else {
        ctx.lineTo(x, y);
      }
    }
    
    ctx.stroke();
  };
  
  return (
    <div className="system-monitor">
      <h2>System Monitor (SignalR)</h2>
      
      <div className="connection-status">
        Status: {connectionState}
      </div>
      
      {metrics && (
        <div className="metrics-grid">
          <div className="metric-card">
            <div className="metric-title">CPU</div>
            <div className="metric-value">{metrics.cpu}%</div>
            <div className="metric-meter">
              <div className="meter-fill" style={{ width: `${metrics.cpu}%`, backgroundColor: metrics.cpu > 80 ? 'red' : '#3498db' }}></div>
            </div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Memory</div>
            <div className="metric-value">{metrics.memory}%</div>
            <div className="metric-meter">
              <div className="meter-fill" style={{ width: `${metrics.memory}%` }}></div>
            </div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Disk Usage</div>
            <div className="metric-value">{metrics.diskUsage}%</div>
            <div className="metric-meter">
              <div className="meter-fill" style={{ width: `${metrics.diskUsage}%` }}></div>
            </div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Requests</div>
            <div className="metric-value">{metrics.requests}</div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Network In</div>
            <div className="metric-value">{metrics.networkIn} KB/s</div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Network Out</div>
            <div className="metric-value">{metrics.networkOut} KB/s</div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Active Connections</div>
            <div className="metric-value">{metrics.activeConnections}</div>
          </div>
          
          <div className="metric-card wide">
            <div className="metric-title">Last Update</div>
            <div className="metric-value">{new Date(metrics.timestamp).toLocaleTimeString()}</div>
          </div>
        </div>
      )}
      
      <div className="chart-container">
        <h3>Resource Usage History</h3>
        <div className="chart-legend">
          <div className="legend-item">
            <div className="color-box cpu"></div>
            <span>CPU</span>
          </div>
          <div className="legend-item">
            <div className="color-box memory"></div>
            <span>Memory</span>
          </div>
        </div>
        <canvas ref={chartRef} width="500" height="200"></canvas>
      </div>
    </div>
  );
};

export default SystemMonitorSignalR;
```

## 3. Server-Sent Events (SSE) Monitoring Example

### Backend Implementation (C#)

```csharp
// MonitoringSSEService.cs

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

public class MonitoringSSEService
{
    private readonly Random _random = new();
    
    // Handle SSE connection
    public async Task HandleSSEConnection(HttpContext context)
    {
        // Set SSE headers
        context.Response.Headers.Append("Content-Type", "text/event-stream");
        context.Response.Headers.Append("Cache-Control", "no-cache");
        context.Response.Headers.Append("Connection", "keep-alive");
        
        // Cancel the operation when client disconnects
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        
        try
        {
            // Send initial connection event
            await SendEvent(context.Response, "connected", new { 
                message = "Monitoring started",
                timestamp = DateTime.UtcNow
            });
            
            // Send metrics every 2 seconds
            while (!cts.Token.IsCancellationRequested)
            {
                var metrics = GenerateMetrics();
                
                await SendEvent(context.Response, "metrics", metrics);
                
                // Wait for 2 seconds
                await Task.Delay(TimeSpan.FromSeconds(2), cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected, no need to do anything
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in SSE stream: {ex.Message}");
            
            // Try to send an error event if we still can
            try
            {
                await SendEvent(context.Response, "error", new {
                    message = "An error occurred in the monitoring stream",
                    timestamp = DateTime.UtcNow
                });
            }
            catch
            {
                // Ignore as connection might already be broken
            }
        }
    }
    
    // Helper to send SSE events
    private async Task SendEvent(HttpResponse response, string eventName, object data)
    {
        var json = JsonSerializer.Serialize(data);
        
        await response.WriteAsync($"event: {eventName}\n");
        await response.WriteAsync($"data: {json}\n\n");
        await response.Body.FlushAsync();
    }
    
    // Generate random metrics for simulation
    private object GenerateMetrics()
    {
        return new
        {
            timestamp = DateTime.UtcNow,
            cpu = _random.Next(10, 90),
            memory = _random.Next(30, 80),
            requests = _random.Next(100, 1000),
            activeConnections = _random.Next(5, 50),
            diskUsage = _random.Next(40, 95),
            networkIn = _random.Next(100, 5000),
            networkOut = _random.Next(100, 5000)
        };
    }
}
```

### Frontend Implementation (TypeScript/React)

```tsx
// SystemMonitorSSE.tsx

import React, { useEffect, useRef, useState } from 'react';

interface SystemMetrics {
  timestamp: string;
  cpu: number;
  memory: number;
  requests: number;
  activeConnections: number;
  diskUsage: number;
  networkIn: number;
  networkOut: number;
}

const SystemMonitorSSE: React.FC = () => {
  const [metrics, setMetrics] = useState<SystemMetrics | null>(null);
  const [connectionState, setConnectionState] = useState<string>('Disconnected');
  const eventSource = useRef<EventSource | null>(null);
  const chartRef = useRef<HTMLCanvasElement | null>(null);
  const cpuHistory = useRef<number[]>([]);
  const memoryHistory = useRef<number[]>([]);
  
  // Connect to SSE endpoint
  useEffect(() => {
    eventSource.current = new EventSource('/api/monitoring/metrics');
    
    // Connection opened
    eventSource.current.onopen = () => {
      setConnectionState('Connected');
    };
    
    // Connection error
    eventSource.current.onerror = (error) => {
      console.error('SSE error:', error);
      
      if (eventSource.current?.readyState === EventSource.CONNECTING) {
        setConnectionState('Reconnecting');
      } else {
        setConnectionState('Error');
      }
    };
    
    // Listen for the metrics event
    eventSource.current.addEventListener('metrics', (event) => {
      try {
        const data = JSON.parse(event.data) as SystemMetrics;
        setMetrics(data);
        
        // Update history arrays
        cpuHistory.current.push(data.cpu);
        memoryHistory.current.push(data.memory);
        
        if (cpuHistory.current.length > 20) {
          cpuHistory.current.shift();
          memoryHistory.current.shift();
        }
        
        // Update chart
        updateChart();
      } catch (error) {
        console.error('Error parsing metrics:', error);
      }
    });
    
    // Listen for the connected event
    eventSource.current.addEventListener('connected', () => {
      setConnectionState('Connected');
    });
    
    // Listen for the error event from server
    eventSource.current.addEventListener('error', (event) => {
      try {
        const data = JSON.parse(event.data);
        console.error('Server reported error:', data.message);
      } catch {
        console.error('Server reported an error');
      }
    });
    
    // Cleanup
    return () => {
      eventSource.current?.close();
    };
  }, []);
  
  // Chart drawing function
  const updateChart = () => {
    const canvas = chartRef.current;
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    
    // Clear canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    // Draw CPU line (red)
    ctx.beginPath();
    ctx.strokeStyle = 'red';
    ctx.lineWidth = 2;
    
    const cpuData = cpuHistory.current;
    const width = canvas.width / (cpuData.length - 1);
    
    for (let i = 0; i < cpuData.length; i++) {
      const x = i * width;
      const y = canvas.height - (cpuData[i] / 100 * canvas.height);
      
      if (i === 0) {
        ctx.moveTo(x, y);
      } else {
        ctx.lineTo(x, y);
      }
    }
    
    ctx.stroke();
    
    // Draw Memory line (blue)
    ctx.beginPath();
    ctx.strokeStyle = 'blue';
    
    const memData = memoryHistory.current;
    
    for (let i = 0; i < memData.length; i++) {
      const x = i * width;
      const y = canvas.height - (memData[i] / 100 * canvas.height);
      
      if (i === 0) {
        ctx.moveTo(x, y);
      } else {
        ctx.lineTo(x, y);
      }
    }
    
    ctx.stroke();
  };
  
  return (
    <div className="system-monitor">
      <h2>System Monitor (SSE)</h2>
      
      <div className="connection-status">
        Status: {connectionState}
      </div>
      
      {metrics && (
        <div className="metrics-grid">
          <div className="metric-card">
            <div className="metric-title">CPU</div>
            <div className="metric-value">{metrics.cpu}%</div>
            <div className="metric-meter">
              <div className="meter-fill" style={{ width: `${metrics.cpu}%`, backgroundColor: metrics.cpu > 80 ? 'red' : '#3498db' }}></div>
            </div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Memory</div>
            <div className="metric-value">{metrics.memory}%</div>
            <div className="metric-meter">
              <div className="meter-fill" style={{ width: `${metrics.memory}%` }}></div>
            </div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Disk Usage</div>
            <div className="metric-value">{metrics.diskUsage}%</div>
            <div className="metric-meter">
              <div className="meter-fill" style={{ width: `${metrics.diskUsage}%` }}></div>
            </div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Requests</div>
            <div className="metric-value">{metrics.requests}</div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Network In</div>
            <div className="metric-value">{metrics.networkIn} KB/s</div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Network Out</div>
            <div className="metric-value">{metrics.networkOut} KB/s</div>
          </div>
          
          <div className="metric-card">
            <div className="metric-title">Active Connections</div>
            <div className="metric-value">{metrics.activeConnections}</div>
          </div>
          
          <div className="metric-card wide">
            <div className="metric-title">Last Update</div>
            <div className="metric-value">{new Date(metrics.timestamp).toLocaleTimeString()}</div>
          </div>
        </div>
      )}
      
      <div className="chart-container">
        <h3>Resource Usage History</h3>
        <div className="chart-legend">
          <div className="legend-item">
            <div className="color-box cpu"></div>
            <span>CPU</span>
          </div>
          <div className="legend-item">
            <div className="color-box memory"></div>
            <span>Memory</span>
          </div>
        </div>
        <canvas ref={chartRef} width="500" height="200"></canvas>
      </div>
    </div>
  );
};

export default SystemMonitorSSE;
```

## 4. CSS for the Monitoring UI

```css
.system-monitor {
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  margin-bottom: 30px;
}

.connection-status {
  padding: 8px 16px;
  background-color: #e9ecef;
  border-radius: 4px;
  margin-bottom: 20px;
  font-weight: bold;
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  grid-gap: 15px;
  margin-bottom: 20px;
}

.metric-card {
  background: white;
  border-radius: 4px;
  padding: 15px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.metric-card.wide {
  grid-column: span 2;
}

.metric-title {
  color: #6c757d;
  font-size: 14px;
  margin-bottom: 8px;
}

.metric-value {
  font-size: 24px;
  font-weight: bold;
  margin-bottom: 10px;
}

.metric-meter {
  height: 8px;
  background-color: #e9ecef;
  border-radius: 4px;
  overflow: hidden;
}

.meter-fill {
  height: 100%;
  background-color: #3498db;
  transition: width 0.5s ease-out;
}

.chart-container {
  background: white;
  border-radius: 4px;
  padding: 15px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.chart-legend {
  display: flex;
  margin-bottom: 10px;
}

.legend-item {
  display: flex;
  align-items: center;
  margin-right: 20px;
}

.color-box {
  width: 12px;
  height: 12px;
  margin-right: 8px;
}

.color-box.cpu {
  background-color: red;
}

.color-box.memory {
  background-color: blue;
}

canvas {
  max-width: 100%;
  background-color: #f8f9fa;
}
```

## Implementation Notes

### Common Patterns

All three implementations share these core features:

1. **Server Timers**: Each uses a timer to generate and push metrics at regular intervals
2. **Client-Side History**: Each maintains an array of historical values for charting
3. **Canvas Rendering**: Each uses the same Canvas API code for visualizing the metrics
4. **Connection State**: Each tracks and displays connection states
5. **Metric Cards**: Each uses the same UI components for displaying metrics

### Key Differences

1. **WebSockets**:
   - Manual handling of connection state
   - JSON message parsing for each message
   - Direct socket.send() for bidirectional capability

2. **SignalR**:
   - Automatic reconnection handling
   - Method-based messaging (ReceiveMetrics)
   - More structured connection state management

3. **Server-Sent Events**:
   - Native browser reconnection
   - Named events (metrics, connected, error)
   - One-way communication only

## Usage Recommendations

- **WebSockets**: Use when you need bidirectional communication, like admin panels that can also send commands
- **SignalR**: Use when you need the most robust solution with reconnection and fallbacks
- **SSE**: Use when you only need server-to-client updates and want the simplest implementation
