// BookHub.cs
// SignalR hub for real-time book updates

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BookAPI.Hubs
{
    // Model for book data
    public class Book
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public bool Availability { get; set; }
        public string LastUpdated { get; set; } = DateTime.UtcNow.ToString("o");
    }
    
    // SignalR Hub for book-related real-time updates
    public class BookHub : Hub
    {
        private readonly ILogger<BookHub> _logger;
        private static readonly Dictionary<string, HashSet<string>> _bookSubscriptions = new();
        
        // Static list of active connections and user information
        private static readonly Dictionary<string, string> _connections = new();
        
        // This would typically be a service injected to access your books database
        // For simplicity, we're using a static list here
        private static readonly List<Book> _books = new()
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
        
        public BookHub(ILogger<BookHub> logger)
        {
            _logger = logger;
        }
        
        // Called when a new client connects
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            _connections[connectionId] = "Anonymous User"; // In a real app, get from auth context
            
            _logger.LogInformation("Client connected: {ConnectionId}", connectionId);
            
            await UpdateActiveUsers();
            await base.OnConnectedAsync();
        }
        
        // Called when a client disconnects
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            _connections.Remove(connectionId);
            
            // Remove user from all book subscriptions
            foreach (var bookId in _bookSubscriptions.Keys)
            {
                if (_bookSubscriptions.TryGetValue(bookId, out var subscribers))
                {
                    subscribers.Remove(connectionId);
                }
            }
            
            _logger.LogInformation("Client disconnected: {ConnectionId}", connectionId);
            
            await UpdateActiveUsers();
            await base.OnDisconnectedAsync(exception);
        }
        
        // Get all books - called by clients to get initial data
        public List<Book> GetAllBooks()
        {
            return _books;
        }
        
        // Update active users count
        private async Task UpdateActiveUsers()
        {
            await Clients.All.SendAsync("ActiveUsersChanged", _connections.Count);
        }
        
        // Join a group to receive all book updates
        public async Task JoinBookUpdatesGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "BookUpdates");
            _logger.LogInformation("Client {ConnectionId} joined BookUpdates group", Context.ConnectionId);
        }
        
        // Leave the book updates group
        public async Task LeaveBookUpdatesGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "BookUpdates");
            _logger.LogInformation("Client {ConnectionId} left BookUpdates group", Context.ConnectionId);
        }
        
        // Subscribe to updates for a specific book
        public Task SubscribeToBook(string bookId)
        {
            if (!_bookSubscriptions.ContainsKey(bookId))
            {
                _bookSubscriptions[bookId] = new HashSet<string>();
            }
            
            _bookSubscriptions[bookId].Add(Context.ConnectionId);
            
            _logger.LogInformation("Client {ConnectionId} subscribed to book {BookId}", 
                Context.ConnectionId, bookId);
                
            return Task.CompletedTask;
        }
        
        // Unsubscribe from updates for a specific book
        public Task UnsubscribeFromBook(string bookId)
        {
            if (_bookSubscriptions.TryGetValue(bookId, out var subscribers))
            {
                subscribers.Remove(Context.ConnectionId);
                _logger.LogInformation("Client {ConnectionId} unsubscribed from book {BookId}", 
                    Context.ConnectionId, bookId);
            }
            
            return Task.CompletedTask;
        }
        
        // Update book availability - called by clients
        public async Task UpdateBookAvailability(string bookId, bool isAvailable)
        {
            _logger.LogInformation("Updating availability for book {BookId} to {IsAvailable}", 
                bookId, isAvailable);
                
            // Find and update the book
            var book = _books.Find(b => b.Id == bookId);
            if (book == null)
            {
                _logger.LogWarning("Book not found: {BookId}", bookId);
                return;
            }
            
            // Update the book
            book.Availability = isAvailable;
            book.LastUpdated = DateTime.UtcNow.ToString("o");
            
            // Notify subscribers about the update
            await NotifyBookUpdated(book);
        }
        
        // Add a new book - would typically be called from a controller
        public async Task AddBook(Book book)
        {
            // Set default values for new book
            book.Id ??= Guid.NewGuid().ToString();
            book.LastUpdated = DateTime.UtcNow.ToString("o");
            
            // Add to the collection
            _books.Add(book);
            
            // Notify clients
            await Clients.Group("BookUpdates").SendAsync("BookAdded", book);
            
            _logger.LogInformation("New book added: {BookTitle} ({BookId})", book.Title, book.Id);
        }
        
        // Notify about book updates
        private async Task NotifyBookUpdated(Book book)
        {
            // Notify group subscribers
            await Clients.Group("BookUpdates").SendAsync("BookUpdated", book);
            
            // Notify specific book subscribers if any
            if (_bookSubscriptions.TryGetValue(book.Id, out var subscribers) && subscribers.Count > 0)
            {
                var subscriberConnections = subscribers.ToList();
                await Clients.Clients(subscriberConnections).SendAsync("BookUpdated", book);
            }
            
            _logger.LogInformation("Book updated notification sent: {BookTitle} ({BookId})", 
                book.Title, book.Id);
        }
        
        // Delete a book - would typically be called from a controller
        public async Task DeleteBook(string bookId)
        {
            var bookIndex = _books.FindIndex(b => b.Id == bookId);
            if (bookIndex == -1)
            {
                _logger.LogWarning("Book not found for deletion: {BookId}", bookId);
                return;
            }
            
            // Remove the book
            _books.RemoveAt(bookIndex);
            
            // Notify clients
            await Clients.Group("BookUpdates").SendAsync("BookDeleted", bookId);
            
            // Notify specific subscribers if any
            if (_bookSubscriptions.TryGetValue(bookId, out var subscribers) && subscribers.Count > 0)
            {
                var subscriberConnections = subscribers.ToList();
                await Clients.Clients(subscriberConnections).SendAsync("BookDeleted", bookId);
                
                // Remove the subscription entry
                _bookSubscriptions.Remove(bookId);
            }
            
            _logger.LogInformation("Book deleted: {BookId}", bookId);
        }
    }
}
