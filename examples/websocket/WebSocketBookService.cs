// WebSocketBookService.cs
// WebSocket implementation for real-time book updates in a .NET minimal API

using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BookAPI.Services
{
    public class WebSocketBookService
    {
        private readonly ILogger<WebSocketBookService> _logger;
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
        private readonly ConcurrentDictionary<string, HashSet<string>> _bookSubscriptions = new();

        public WebSocketBookService(ILogger<WebSocketBookService> logger)
        {
            _logger = logger;
        }

        // Map the WebSocket endpoint
        public void MapWebSocketEndpoint(WebApplication app)
        {
            app.UseWebSockets(); // Enable WebSockets middleware
            
            app.Map("/books-ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var socketId = Guid.NewGuid().ToString();
                    
                    _logger.LogInformation("WebSocket connection established: {SocketId}", socketId);
                    _sockets.TryAdd(socketId, webSocket);
                    
                    await HandleWebSocketConnection(socketId, webSocket);
                    
                    // Remove socket when disconnected
                    _sockets.TryRemove(socketId, out _);
                    
                    // Clean up any subscriptions
                    foreach (var bookId in _bookSubscriptions.Keys)
                    {
                        if (_bookSubscriptions.TryGetValue(bookId, out var subscribers))
                        {
                            subscribers.Remove(socketId);
                        }
                    }
                    
                    _logger.LogInformation("WebSocket connection closed: {SocketId}", socketId);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            });
        }
        
        // Handle an active WebSocket connection
        private async Task HandleWebSocketConnection(string socketId, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4]; // 4KB buffer
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
            
            // Send initial book data
            await SendInitialBookData(socketId);
            
            // Process incoming messages until the socket is closed
            while (!receiveResult.CloseStatus.HasValue)
            {
                // Process the received message
                string message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                await ProcessWebSocketMessage(socketId, message);
                
                // Get the next message
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            
            // Close the WebSocket gracefully
            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
        
        // Process a message received from a WebSocket client
        private async Task ProcessWebSocketMessage(string socketId, string message)
        {
            try
            {
                var messageObj = JsonSerializer.Deserialize<JsonElement>(message);
                var messageType = messageObj.GetProperty("type").GetString();
                
                switch (messageType)
                {
                    case "subscribe":
                        var channel = messageObj.GetProperty("channel").GetString();
                        
                        if (channel == "book-updates")
                        {
                            // Subscribe to all book updates
                            _logger.LogInformation("Client {SocketId} subscribed to all book updates", socketId);
                            
                            // You would typically add this socket to a "global subscribers" list
                        }
                        else if (channel == "book" && messageObj.TryGetProperty("bookId", out var bookIdElement))
                        {
                            // Subscribe to a specific book
                            var bookId = bookIdElement.GetString();
                            if (string.IsNullOrEmpty(bookId))
                                return;
                                
                            _logger.LogInformation("Client {SocketId} subscribed to book {BookId}", socketId, bookId);
                            
                            // Add to book subscriptions
                            _bookSubscriptions.AddOrUpdate(
                                bookId,
                                _ => new HashSet<string> { socketId },
                                (_, subscribers) =>
                                {
                                    subscribers.Add(socketId);
                                    return subscribers;
                                });
                        }
                        break;
                        
                    case "unsubscribe":
                        if (messageObj.TryGetProperty("bookId", out var unsubBookIdElement))
                        {
                            var bookId = unsubBookIdElement.GetString();
                            if (string.IsNullOrEmpty(bookId))
                                return;
                                
                            if (_bookSubscriptions.TryGetValue(bookId, out var subscribers))
                            {
                                subscribers.Remove(socketId);
                                _logger.LogInformation("Client {SocketId} unsubscribed from book {BookId}", socketId, bookId);
                            }
                        }
                        break;
                        
                    case "update-book":
                        if (messageObj.TryGetProperty("bookId", out var updateBookIdElement) &&
                            messageObj.TryGetProperty("changes", out var changesElement))
                        {
                            var bookId = updateBookIdElement.GetString();
                            
                            // Here you would update the book in your database
                            // For this example, we'll simulate it and broadcast the change
                            
                            // In a real application you'd verify the user has permission to update
                            // and then update the actual database record
                            
                            _logger.LogInformation("Book update request from {SocketId} for book {BookId}", 
                                socketId, bookId);
                                
                            // Simulate successful update and broadcast to all subscribers
                            await NotifyBookUpdated(bookId);
                        }
                        break;
                        
                    default:
                        _logger.LogWarning("Unknown message type: {MessageType}", messageType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing WebSocket message: {Message}", message);
            }
        }
        
        // Send initial data to a new connection
        private async Task SendInitialBookData(string socketId)
        {
            if (_sockets.TryGetValue(socketId, out var socket))
            {
                try
                {
                    // In a real application, you would fetch books from your database
                    // For example, using a BookRepository or similar
                    
                    // Simulated book data
                    var books = new[]
                    {
                        new 
                        {
                            id = "1",
                            title = "The Great Gatsby",
                            author = "F. Scott Fitzgerald",
                            availability = true,
                            lastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
                        },
                        new 
                        {
                            id = "2",
                            title = "To Kill a Mockingbird",
                            author = "Harper Lee",
                            availability = false,
                            lastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
                        },
                        new 
                        {
                            id = "3",
                            title = "1984",
                            author = "George Orwell",
                            availability = true,
                            lastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
                        }
                    };
                    
                    var response = new
                    {
                        type = "initial-data",
                        data = new
                        {
                            books
                        }
                    };
                    
                    var responseJson = JsonSerializer.Serialize(response);
                    var responseBytes = Encoding.UTF8.GetBytes(responseJson);
                    
                    await socket.SendAsync(
                        new ArraySegment<byte>(responseBytes),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending initial data to {SocketId}", socketId);
                }
            }
        }
        
        // Notify all clients about a book being added
        public async Task NotifyBookAdded(object book)
        {
            var message = new
            {
                type = "book-added",
                data = new
                {
                    book
                }
            };
            
            await BroadcastToAll(JsonSerializer.Serialize(message));
        }
        
        // Notify clients about a book being updated
        public async Task NotifyBookUpdated(string bookId)
        {
            // In a real app, you would fetch the updated book from your database
            // Simulated updated book - in reality you would retrieve the actual updated book
            var updatedBook = new
            {
                id = bookId,
                title = "Updated Book Title",
                author = "Updated Author",
                availability = true, // Simulating a toggle to available
                lastUpdated = DateTime.UtcNow.ToString("o")
            };
            
            var message = new
            {
                type = "book-updated",
                data = new
                {
                    book = updatedBook
                }
            };
            
            var messageJson = JsonSerializer.Serialize(message);
            
            // Send to subscribers of this specific book
            if (_bookSubscriptions.TryGetValue(bookId, out var subscribers))
            {
                foreach (var socketId in subscribers)
                {
                    await SendToClient(socketId, messageJson);
                }
            }
            
            // Also broadcast to clients subscribed to all updates
            await BroadcastToAll(messageJson);
        }
        
        // Notify clients about a book being deleted
        public async Task NotifyBookDeleted(string bookId)
        {
            var message = new
            {
                type = "book-deleted",
                data = new
                {
                    bookId
                }
            };
            
            await BroadcastToAll(JsonSerializer.Serialize(message));
        }
        
        // Send a message to a specific client
        private async Task SendToClient(string socketId, string message)
        {
            if (_sockets.TryGetValue(socketId, out var socket) && 
                socket.State == WebSocketState.Open)
            {
                try
                {
                    var messageBytes = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(
                        new ArraySegment<byte>(messageBytes),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending message to {SocketId}", socketId);
                    
                    // Remove dead connections
                    _sockets.TryRemove(socketId, out _);
                }
            }
        }
        
        // Broadcast a message to all connected clients
        private async Task BroadcastToAll(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var deadSockets = new List<string>();
            
            foreach (var (socketId, socket) in _sockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await socket.SendAsync(
                            new ArraySegment<byte>(messageBytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error broadcasting to {SocketId}", socketId);
                        deadSockets.Add(socketId);
                    }
                }
                else
                {
                    deadSockets.Add(socketId);
                }
            }
            
            // Clean up dead connections
            foreach (var socketId in deadSockets)
            {
                _sockets.TryRemove(socketId, out _);
            }
        }
    }
    
    // Extension method for easy registration in Program.cs
    public static class WebSocketBookServiceExtensions
    {
        public static IServiceCollection AddWebSocketBookService(this IServiceCollection services)
        {
            services.AddSingleton<WebSocketBookService>();
            return services;
        }
        
        public static WebApplication UseWebSocketBookService(this WebApplication app)
        {
            var service = app.Services.GetRequiredService<WebSocketBookService>();
            service.MapWebSocketEndpoint(app);
            return app;
        }
    }
}
