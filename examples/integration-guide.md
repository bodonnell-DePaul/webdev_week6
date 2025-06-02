# Integration Guide: Adding Real-Time Features to the Book Manager Application

This guide explains how to integrate real-time features into the existing Book Manager application using WebSockets, SignalR, and SSE.

## Table of Contents

1. [Project Structure](#project-structure)
2. [Backend Integration Steps](#backend-integration)
3. [Frontend Integration Steps](#frontend-integration)
4. [Testing Real-Time Features](#testing)
5. [Advanced Scenarios](#advanced-scenarios)

## Project Structure <a name="project-structure"></a>

The Book Manager application consists of:

- **Frontend**: React application in `/frontend/book-manager`
- **Backend**: .NET Minimal API in `/backend/BookAPI`

We'll add real-time capabilities to provide:

1. Real-time book inventory updates
2. Real-time notifications when books are added/modified/deleted
3. Live user activity indicators
4. Instant search results

## Backend Integration <a name="backend-integration"></a>

### Step 1: Add Required NuGet Packages

```bash
cd /home/bodonnell/lectures/webdev_week8/backend/BookAPI
dotnet add package Microsoft.AspNetCore.SignalR
```

### Step 2: Modify Program.cs to Add Real-Time Services

Open `Program.cs` and add the following services:

```csharp
// Add CORS for WebSocket and SignalR
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

// Add SignalR
builder.Services.AddSignalR();

// Add WebSocket support
builder.Services.AddWebSocketBookService();

// Add SSE support
builder.Services.AddSSEBookService();

// Configure middleware
app.UseCors("CorsPolicy");

// Configure WebSockets
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

// Map SignalR hub
app.MapHub<BookHub>("/bookHub");

// Configure WebSocket endpoints
app.UseWebSocketBookService();

// Configure SSE endpoints
app.UseSSEBookService();
```

### Step 3: Copy Service Files to the BookAPI Project

1. Copy the following files to your project:
   - `WebSocketBookService.cs` to `/backend/BookAPI/Services/WebSocketBookService.cs`
   - `BookHub.cs` to `/backend/BookAPI/Hubs/BookHub.cs`  
   - `SSEBookService.cs` to `/backend/BookAPI/Services/SSEBookService.cs`

2. Make sure to update the namespaces to match your project structure.

### Step 4: Integrate with Existing Book Operations

Modify your existing book controller/endpoint methods to trigger real-time updates:

```csharp
// Example: Adding a book
app.MapPost("/api/books", async (Book book, BookService bookService, 
    WebSocketBookService wsService, SSEBookService sseService, 
    IHubContext<BookHub> hubContext) =>
{
    // Add book using existing service
    var newBook = await bookService.AddBook(book);
    
    // Broadcast via SignalR
    await hubContext.Clients.Group("BookUpdates").SendAsync("BookAdded", newBook);
    
    // Broadcast via WebSockets
    await wsService.NotifyBookAdded(newBook);
    
    // Broadcast via SSE
    sseService.BroadcastBookAdded(newBook);
    
    return Results.Created($"/api/books/{newBook.Id}", newBook);
});

// Similar modifications for PUT, DELETE etc.
```

## Frontend Integration <a name="frontend-integration"></a>

### Step 1: Add Required NPM Packages

```bash
cd /home/bodonnell/lectures/webdev_week8/frontend/book-manager
npm install @microsoft/signalr
```

### Step 2: Create Real-Time Service Files

1. Copy the client implementation files to your frontend project:
   - `WebSocketClient.ts` to `/frontend/book-manager/src/services/WebSocketClient.ts`
   - `SignalRService.ts` to `/frontend/book-manager/src/services/SignalRService.ts`
   - `SSEClient.ts` to `/frontend/book-manager/src/services/SSEClient.ts`

2. Create a unified real-time service that uses all three technologies:

```typescript
// src/services/RealTimeService.ts
import { useSignalR } from './SignalRService';
import { useWebSocket } from './WebSocketClient';
import { useSSE } from './SSEClient';
import { useEffect, useState } from 'react';
import { Book } from '../types/Book';

// This is just one approach - select a single technology based on your needs
export const useRealTimeBooks = (initialBooks: Book[] = []) => {
  const [books, setBooks] = useState<Book[]>(initialBooks);
  const [connectionState, setConnectionState] = useState<string>('disconnected');
  
  // Configure endpoints - adjust URLs based on your setup
  const { connectionState: signalRState, on: signalROn } = 
    useSignalR('https://localhost:5001/bookHub');
  
  const { connectionState: wsState, messages: wsMessages } = 
    useWebSocket('ws://localhost:5000/books-ws');
  
  const { connectionState: sseState, addEventListener: sseAddEventListener } = 
    useSSE('https://localhost:5001/api/books/events');
  
  // Determine overall connection state
  useEffect(() => {
    if (signalRState === 'connected' || wsState === 'connected' || sseState === 'connected') {
      setConnectionState('connected');
    } else if (signalRState === 'connecting' || wsState === 'connecting' || sseState === 'connecting') {
      setConnectionState('connecting');
    } else {
      setConnectionState('disconnected');
    }
  }, [signalRState, wsState, sseState]);
  
  // Set up SignalR event handlers
  useEffect(() => {
    if (signalRState === 'connected') {
      // Book added handler
      signalROn('BookAdded', (book: Book) => {
        setBooks(currentBooks => [...currentBooks, book]);
      });
      
      // Book updated handler
      signalROn('BookUpdated', (book: Book) => {
        setBooks(currentBooks => 
          currentBooks.map(b => b.id === book.id ? book : b)
        );
      });
      
      // Book deleted handler
      signalROn('BookDeleted', (bookId: string) => {
        setBooks(currentBooks => 
          currentBooks.filter(b => b.id !== bookId)
        );
      });
    }
  }, [signalRState, signalROn]);
  
  // Handle WebSocket messages
  useEffect(() => {
    if (wsMessages.length > 0) {
      const latestMessage = wsMessages[wsMessages.length - 1];
      
      switch (latestMessage.type) {
        case 'book-added':
          setBooks(currentBooks => [...currentBooks, latestMessage.data.book]);
          break;
          
        case 'book-updated':
          setBooks(currentBooks => 
            currentBooks.map(b => 
              b.id === latestMessage.data.book.id ? latestMessage.data.book : b
            )
          );
          break;
          
        case 'book-deleted':
          setBooks(currentBooks => 
            currentBooks.filter(b => b.id !== latestMessage.data.bookId)
          );
          break;
      }
    }
  }, [wsMessages]);
  
  // Set up SSE event listeners
  useEffect(() => {
    const cleanup: (() => void)[] = [];
    
    if (sseState === 'connected') {
      // Book added handler
      const bookAddedCleanup = sseAddEventListener('book-added', (data) => {
        setBooks(currentBooks => [...currentBooks, data.book]);
      });
      cleanup.push(bookAddedCleanup);
      
      // Book updated handler
      const bookUpdatedCleanup = sseAddEventListener('book-updated', (data) => {
        setBooks(currentBooks => 
          currentBooks.map(b => b.id === data.book.id ? data.book : b)
        );
      });
      cleanup.push(bookUpdatedCleanup);
      
      // Book deleted handler
      const bookDeletedCleanup = sseAddEventListener('book-deleted', (data) => {
        setBooks(currentBooks => 
          currentBooks.filter(b => b.id !== data.bookId)
        );
      });
      cleanup.push(bookDeletedCleanup);
    }
    
    return () => {
      cleanup.forEach(fn => fn());
    };
  }, [sseState, sseAddEventListener]);
  
  return { books, connectionState };
};
```

### Step 3: Update Book Components to Use Real-Time Data

Modify your book list component to use the real-time service:

```tsx
// src/components/BookList.tsx
import React, { useEffect, useState } from 'react';
import { useRealTimeBooks } from '../services/RealTimeService';
import { Book } from '../types/Book';
import BookService from '../services/BookService';

const BookList: React.FC = () => {
  const [initialBooks, setInitialBooks] = useState<Book[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  
  // Load initial books
  useEffect(() => {
    const fetchBooks = async () => {
      try {
        const books = await BookService.getBooks();
        setInitialBooks(books);
      } catch (error) {
        console.error('Error fetching books:', error);
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchBooks();
  }, []);
  
  // Use our real-time books hook
  const { books, connectionState } = useRealTimeBooks(initialBooks);
  
  if (isLoading) {
    return <div>Loading books...</div>;
  }
  
  return (
    <div className="book-list">
      <div className="connection-status">
        Connection: {connectionState}
      </div>
      
      <h2>Books</h2>
      <table>
        <thead>
          <tr>
            <th>Title</th>
            <th>Author</th>
            <th>Availability</th>
            <th>Last Updated</th>
          </tr>
        </thead>
        <tbody>
          {books.map(book => (
            <tr key={book.id}>
              <td>{book.title}</td>
              <td>{book.author}</td>
              <td>
                <span className={book.availability ? 'available' : 'unavailable'}>
                  {book.availability ? 'Available' : 'Unavailable'}
                </span>
              </td>
              <td>{new Date(book.lastUpdated).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default BookList;
```

### Step 4: Add a Notifications Component

Create a component to show real-time notifications:

```tsx
// src/components/Notifications.tsx
import React, { useEffect, useState } from 'react';
import { useSignalR } from '../services/SignalRService';
import { useSSE } from '../services/SSEClient';

const Notifications: React.FC = () => {
  const [notifications, setNotifications] = useState<Array<{id: string, message: string, time: Date}>>([]);
  const { connectionState: signalRState, on: signalROn } = useSignalR('https://localhost:5001/bookHub');
  const { connectionState: sseState, addEventListener } = useSSE('https://localhost:5001/api/books/events');
  
  // SignalR notifications
  useEffect(() => {
    if (signalRState === 'connected') {
      signalROn('BookAdded', (book) => {
        addNotification(`Book added: "${book.title}"`);
      });
      
      signalROn('BookUpdated', (book) => {
        addNotification(`Book updated: "${book.title}"`);
      });
      
      signalROn('BookDeleted', (bookId) => {
        addNotification(`Book deleted (ID: ${bookId})`);
      });
      
      signalROn('ActiveUsersChanged', (count) => {
        addNotification(`Active users: ${count}`);
      });
    }
  }, [signalRState, signalROn]);
  
  // SSE notifications
  useEffect(() => {
    const cleanupFns: Array<() => void> = [];
    
    if (sseState === 'connected') {
      const bookAddedCleanup = addEventListener('book-added', (data) => {
        addNotification(`SSE: Book added - "${data.book.title}"`);
      });
      cleanupFns.push(bookAddedCleanup);
      
      const bookUpdatedCleanup = addEventListener('book-updated', (data) => {
        addNotification(`SSE: Book updated - "${data.book.title}"`);
      });
      cleanupFns.push(bookUpdatedCleanup);
    }
    
    return () => {
      cleanupFns.forEach(fn => fn());
    };
  }, [sseState, addEventListener]);
  
  // Add notification helper
  const addNotification = (message: string) => {
    setNotifications(prev => [
      { id: Date.now().toString(), message, time: new Date() },
      ...prev
    ].slice(0, 5)); // Keep only 5 most recent
  };
  
  return (
    <div className="notifications">
      <h3>Recent Notifications</h3>
      {notifications.length === 0 ? (
        <p>No notifications yet</p>
      ) : (
        <ul>
          {notifications.map(note => (
            <li key={note.id}>
              <span className="time">{note.time.toLocaleTimeString()}</span>
              <span className="message">{note.message}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};

export default Notifications;
```

## Testing Real-Time Features <a name="testing"></a>

### Test 1: Book Addition

1. Open two browser windows with the application
2. Add a book in one window
3. Verify the book appears in both windows immediately

### Test 2: Book Availability Toggle

1. Open two browser windows with the application
2. Toggle a book's availability in one window
3. Verify the change is reflected in both windows instantly

### Test 3: Multiple Technologies

This test ensures all three technologies are working:

1. Open the browser developer tools network tab
2. Filter for WebSocket, Event Stream, and SignalR connections
3. Verify all three connections are established
4. Make a change to a book and observe all three technologies receiving updates

## Advanced Scenarios <a name="advanced-scenarios"></a>

### Connection Resilience

For production applications, consider enhancing connection resilience:

1. **SignalR Backplane**: For multi-server deployments, use Redis or SQL Server as a backplane:

```csharp
// In Program.cs
builder.Services.AddSignalR().AddStackExchangeRedis("your-redis-connection-string");
```

2. **WebSocket Reconnection**: Implement exponential backoff for reconnection attempts:

```typescript
// Already implemented in our WebSocketClient
// Retry with increasing delays: 1s, 2s, 4s, 8s, 16s
```

3. **SSE Reconnection**: Handle reconnection events:

```typescript
eventSource.onopen = (event) => {
  if (reconnectCount > 0) {
    console.log('SSE reconnected after', reconnectCount, 'attempts');
    reconnectCount = 0;
  }
};
```

### Performance Optimization

1. **Message Batching**: For high-frequency updates, batch messages:

```typescript
// Accumulate changes
const pendingChanges = [];

// Send batched updates every 100ms
setInterval(() => {
  if (pendingChanges.length > 0) {
    sendMessage('batch-update', { changes: pendingChanges });
    pendingChanges.length = 0;
  }
}, 100);
```

2. **Message Compression**: For larger payloads:

```csharp
// In Program.cs
app.UseResponseCompression();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});
```

### Security Considerations

1. **Authentication**: Secure your real-time endpoints:

```csharp
// SignalR Authorization
app.MapHub<BookHub>("/bookHub").RequireAuthorization();
```

2. **Input Validation**: Validate all incoming messages:

```csharp
public async Task UpdateBook(Book book)
{
    if (!ModelState.IsValid)
    {
        throw new HubException("Invalid book data");
    }
    
    // Process update
}
```

3. **Rate Limiting**: Implement rate limiting for message sending:

```csharp
private static readonly Dictionary<string, DateTime> _lastMessageTime = new();

private bool RateLimitCheck(string connectionId)
{
    const int messageLimit = 10; // 10 messages
    const int timeWindowSeconds = 5; // per 5 seconds
    
    var now = DateTime.UtcNow;
    
    if (_lastMessageTime.TryGetValue(connectionId, out var lastTime))
    {
        if ((now - lastTime).TotalSeconds < timeWindowSeconds / messageLimit)
        {
            return false; // Rate limit exceeded
        }
    }
    
    _lastMessageTime[connectionId] = now;
    return true;
}
```

## Choosing the Right Technology

Based on your specific needs, you can choose one of the three technologies:

- **WebSockets**: Best for true bidirectional communication with low latency
- **SignalR**: Best for ease of development and automatic fallbacks
- **SSE**: Best for simple, one-way server-to-client updates

Or use a combination of them as demonstrated in this guide!
