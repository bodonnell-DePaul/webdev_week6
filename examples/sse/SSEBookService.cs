// SSEBookService.cs
// Server-Sent Events implementation for real-time book updates

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BookAPI.Services
{
    public class SSEBookService
    {
        private readonly ILogger<SSEBookService> _logger;
        
        // In a real application, this would be backed by a persistent store or database
        private readonly List<Book> _books = new()
        {
            new Book
            {
                Id = "1",
                Title = "The Great Gatsby",
                Author = "F. Scott Fitzgerald",
                Availability = true,
                LastUpdated = DateTime.UtcNow.AddDays(-5).ToString("o")
            },
            new Book
            {
                Id = "2",
                Title = "To Kill a Mockingbird",
                Author = "Harper Lee",
                Availability = false,
                LastUpdated = DateTime.UtcNow.AddDays(-2).ToString("o")
            },
            new Book
            {
                Id = "3",
                Title = "1984",
                Author = "George Orwell",
                Availability = true,
                LastUpdated = DateTime.UtcNow.AddDays(-7).ToString("o")
            }
        };
        
        public SSEBookService(ILogger<SSEBookService> logger)
        {
            _logger = logger;
        }
        
        // Map the SSE endpoint
        public void MapSSEEndpoint(WebApplication app)
        {
            app.MapGet("/api/books/events", HandleSSEConnection);
        }
        
        // Handle SSE connection
        private async Task HandleSSEConnection(HttpContext context)
        {
            // Set headers required for SSE
            context.Response.Headers.Append("Content-Type", "text/event-stream");
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");
            
            // Disable response buffering
            context.Response.Body.Flush();
            
            // Create a cancellation token for this connection
            using var cts = new CancellationTokenSource();
            
            // Cancel the token when the request is aborted
            context.RequestAborted.Register(() => cts.Cancel());
            
            _logger.LogInformation("SSE connection established");
            
            try
            {
                // Send initial event with current timestamp
                await SendEvent(context.Response, "connection-established", 
                    new { timestamp = DateTime.UtcNow });
                
                // Send events until the connection is closed
                while (!cts.Token.IsCancellationRequested)
                {
                    // Keep the connection alive with a comment
                    await context.Response.WriteAsync(": keepalive\n\n");
                    await context.Response.Body.FlushAsync();
                    
                    // Wait for a period to avoid excessive CPU usage
                    await Task.Delay(TimeSpan.FromSeconds(30), cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("SSE connection closed by client");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SSE connection");
            }
        }
        
        // Send an SSE event
        private static async Task SendEvent(HttpResponse response, string eventType, object data, string id = null)
        {
            // Format the SSE event
            var payload = $"event: {eventType}\n";
            
            if (!string.IsNullOrEmpty(id))
            {
                payload += $"id: {id}\n";
            }
            
            var dataJson = JsonSerializer.Serialize(data);
            payload += $"data: {dataJson}\n\n";
            
            await response.WriteAsync(payload);
            await response.Body.FlushAsync();
        }
        
        // Method to broadcast a book added event to all SSE clients
        public void BroadcastBookAdded(Book book)
        {
            // In a real application, you would maintain a list of active SSE connections
            // and send the event to each of them.
            
            // For this example, we're focusing on the event structure
            _books.Add(book);
            _logger.LogInformation("Book added event broadcast: {BookTitle} ({BookId})", book.Title, book.Id);
        }
        
        // Method to broadcast a book updated event
        public void BroadcastBookUpdated(Book book)
        {
            var existingBook = _books.Find(b => b.Id == book.Id);
            if (existingBook != null)
            {
                var index = _books.IndexOf(existingBook);
                _books[index] = book;
                _logger.LogInformation("Book updated event broadcast: {BookTitle} ({BookId})", book.Title, book.Id);
            }
        }
        
        // Method to broadcast a book deleted event
        public void BroadcastBookDeleted(string bookId)
        {
            var book = _books.Find(b => b.Id == bookId);
            if (book != null)
            {
                _books.Remove(book);
                _logger.LogInformation("Book deleted event broadcast: {BookId}", bookId);
            }
        }
        
        // Get all books
        public List<Book> GetAllBooks()
        {
            return _books;
        }
        
        // Update book availability
        public async Task<Book> UpdateBookAvailability(string bookId, bool isAvailable)
        {
            var book = _books.Find(b => b.Id == bookId);
            if (book == null)
            {
                return null;
            }
            
            book.Availability = isAvailable;
            book.LastUpdated = DateTime.UtcNow.ToString("o");
            
            // In a real application, you would update the book in your database
            _logger.LogInformation("Book availability updated: {BookId} to {IsAvailable}", 
                bookId, isAvailable);
            
            return book;
        }
    }
    
    // Book model
    public class Book
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public bool Availability { get; set; }
        public string LastUpdated { get; set; } = DateTime.UtcNow.ToString("o");
    }
    
    // Extension methods for easy registration in Program.cs
    public static class SSEBookServiceExtensions
    {
        public static IServiceCollection AddSSEBookService(this IServiceCollection services)
        {
            services.AddSingleton<SSEBookService>();
            return services;
        }
        
        public static WebApplication UseSSEBookService(this WebApplication app)
        {
            var service = app.Services.GetRequiredService<SSEBookService>();
            service.MapSSEEndpoint(app);
            
            // Map additional endpoints for book operations
            app.MapGet("/api/books", (SSEBookService service) => service.GetAllBooks());
            
            app.MapPut("/api/books/{bookId}/availability", async (string bookId, AvailabilityRequest request, SSEBookService service, HttpContext context) =>
            {
                var book = await service.UpdateBookAvailability(bookId, request.IsAvailable);
                if (book == null)
                {
                    return Results.NotFound();
                }
                
                // Broadcast the update to all SSE clients
                service.BroadcastBookUpdated(book);
                
                return Results.Ok(book);
            });
            
            return app;
        }
    }
    
    // Request model for availability updates
    public class AvailabilityRequest
    {
        public bool IsAvailable { get; set; }
    }
}
