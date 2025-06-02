// Program.cs
// Example of integrating all three real-time technologies in a .NET minimal API

using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BookAPI.Hubs;
using BookAPI.Services;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS for web client
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .WithOrigins("http://localhost:5173") // The React app's URL
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Needed for SignalR
    });
});

// Add services to the container

// Add SignalR
builder.Services.AddSignalR();

// Add WebSocket service
builder.Services.AddWebSocketBookService();

// Add SSE service
builder.Services.AddSSEBookService();

// Add API Endpoints
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configuring CORS - must be before mapping endpoints
app.UseCors("CorsPolicy");

// Configure WebSockets
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

// Map minimal API endpoints for books
app.MapGet("/api/books/list", async (SSEBookService sseService) =>
{
    // Use the SSE service to get books (just as an example)
    // In a real app, you would likely have a separate repository
    return Results.Ok(sseService.GetAllBooks());
});

// Map SignalR hub
app.MapHub<BookHub>("/bookHub");

// Configure WebSocket service
app.UseWebSocketBookService();

// Configure SSE service
app.UseSSEBookService();

// Add a test endpoint that demonstrates triggering events across all technologies
app.MapPost("/api/test/broadcast-book-update", async (
    string bookId,
    IHubContext<BookHub> hubContext,
    WebSocketBookService wsService,
    SSEBookService sseService) =>
{
    try
    {
        // Get a book to update (using SSE service's book list for convenience)
        var books = sseService.GetAllBooks();
        var book = books.FirstOrDefault(b => b.Id == bookId);
        
        if (book == null)
        {
            return Results.NotFound($"No book found with ID {bookId}");
        }
        
        // Toggle availability
        book.Availability = !book.Availability;
        book.LastUpdated = DateTime.UtcNow.ToString("o");
        
        // Broadcast via SignalR
        await hubContext.Clients.Group("BookUpdates").SendAsync("BookUpdated", book);
        
        // Broadcast via WebSockets
        await wsService.NotifyBookUpdated(bookId);
        
        // Broadcast via SSE
        sseService.BroadcastBookUpdated(book);
        
        return Results.Ok(new 
        { 
            message = "Book update broadcast to all channels", 
            book
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error broadcasting update: {ex.Message}");
    }
});

// Run the application
app.Run();
