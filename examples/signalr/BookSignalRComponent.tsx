// BookSignalRComponent.tsx
// Example React component using SignalR for real-time book updates

import React, { useEffect, useState, useCallback } from 'react';
import { useSignalR, Book, BookEvent } from './SignalRService';

// Map SignalR connection states to display strings
const connectionStateMap: Record<string, string> = {
  'disconnected': 'Disconnected',
  'connecting': 'Connecting...',
  'connected': 'Connected',
  'reconnecting': 'Reconnecting...',
  'error': 'Connection Error'
};

// Status indicator component
const ConnectionStatus: React.FC<{ status: string }> = ({ status }) => {
  const getStatusColor = () => {
    switch (status) {
      case 'connected': return 'green';
      case 'connecting': return 'orange';
      case 'reconnecting': return 'orange';
      case 'disconnected': return 'red';
      case 'error': return 'red';
      default: return 'gray';
    }
  };

  return (
    <div className="status-indicator" style={{ 
      display: 'flex', 
      alignItems: 'center', 
      marginBottom: '1rem' 
    }}>
      <div style={{
        width: '10px',
        height: '10px',
        borderRadius: '50%',
        backgroundColor: getStatusColor(),
        marginRight: '0.5rem'
      }}></div>
      <span>Status: {connectionStateMap[status] || status}</span>
    </div>
  );
};

// Book list with real-time updates via SignalR
const SignalRBookList: React.FC = () => {
  // Books state
  const [books, setBooks] = useState<Book[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  
  // Use our SignalR hook
  const { 
    connectionState, 
    error, 
    on, 
    off, 
    invoke, 
    startConnection 
  } = useSignalR('https://localhost:5001/bookHub');
  
  // Load initial book data
  const loadBooks = useCallback(async () => {
    try {
      setIsLoading(true);
      // We can use the SignalR connection to get initial data
      const initialBooks = await invoke('GetAllBooks');
      setBooks(initialBooks);
    } catch (err) {
      console.error('Error loading books:', err);
    } finally {
      setIsLoading(false);
    }
  }, [invoke]);
  
  // Handle book events
  const handleBookEvent = useCallback((event: BookEvent) => {
    console.log('Book event received:', event);
    
    switch (event.operation) {
      case 'added':
        if (event.book) {
          setBooks(prev => [...prev, event.book]);
        }
        break;
        
      case 'updated':
        if (event.book) {
          setBooks(prev => 
            prev.map(book => 
              book.id === event.book?.id ? event.book : book
            )
          );
        }
        break;
        
      case 'deleted':
        if (event.bookId) {
          setBooks(prev => 
            prev.filter(book => book.id !== event.bookId)
          );
        }
        break;
    }
  }, []);
  
  // Set up SignalR event listeners when connection is established
  useEffect(() => {
    if (connectionState === 'connected') {
      // Register for all book events
      on('BookAdded', (book: Book) => {
        handleBookEvent({ operation: 'added', book });
      });
      
      on('BookUpdated', (book: Book) => {
        handleBookEvent({ operation: 'updated', book });
      });
      
      on('BookDeleted', (bookId: string) => {
        handleBookEvent({ operation: 'deleted', bookId });
      });
      
      // Load initial book data
      loadBooks();
      
      // Join the book updates group
      invoke('JoinBookUpdatesGroup');
    }
    
    // Cleanup
    return () => {
      off('BookAdded', () => {});
      off('BookUpdated', () => {});
      off('BookDeleted', () => {});
    };
  }, [connectionState, on, off, invoke, loadBooks, handleBookEvent]);
  
  // Toggle book availability
  const toggleAvailability = async (bookId: string) => {
    const book = books.find(b => b.id === bookId);
    if (book) {
      try {
        // Call the SignalR hub method to update the book
        await invoke('UpdateBookAvailability', bookId, !book.availability);
        // NOTE: We don't update the UI here - we wait for the real-time update from the server
      } catch (err) {
        console.error('Error updating book:', err);
      }
    }
  };
  
  return (
    <div className="signalr-books">
      <h2>SignalR Real-Time Book Management</h2>
      
      {/* Display connection status */}
      <ConnectionStatus status={connectionState} />
      
      {/* Error message if any */}
      {error && <div className="error-message" style={{ color: 'red', marginBottom: '1rem' }}>{error}</div>}
      
      {/* Reconnect button */}
      {(connectionState === 'disconnected' || connectionState === 'error') && (
        <button 
          onClick={startConnection}
          style={{ marginBottom: '1rem' }}
        >
          Reconnect
        </button>
      )}
      
      {/* Book list */}
      <div className="book-list">
        {isLoading ? (
          <p>Loading books...</p>
        ) : books.length === 0 ? (
          <p>No books available</p>
        ) : (
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr>
                <th>Title</th>
                <th>Author</th>
                <th>Availability</th>
                <th>Last Updated</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {books.map(book => (
                <tr key={book.id}>
                  <td>{book.title}</td>
                  <td>{book.author}</td>
                  <td>
                    <span style={{ 
                      color: book.availability ? 'green' : 'red',
                      fontWeight: 'bold'
                    }}>
                      {book.availability ? 'Available' : 'Unavailable'}
                    </span>
                  </td>
                  <td>{new Date(book.lastUpdated).toLocaleString()}</td>
                  <td>
                    <button 
                      onClick={() => toggleAvailability(book.id)}
                      style={{ 
                        backgroundColor: book.availability ? '#f8d7da' : '#d4edda',
                        border: 'none',
                        padding: '5px 10px',
                        borderRadius: '3px',
                        cursor: 'pointer'
                      }}
                    >
                      {book.availability ? 'Mark Unavailable' : 'Mark Available'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
      
      {/* Real-time user activity indicator */}
      <div className="user-activity" style={{ marginTop: '2rem', padding: '1rem', backgroundColor: '#f8f9fa', borderRadius: '5px' }}>
        <h3>Active Users</h3>
        <div id="active-users">
          {/* This will be populated by SignalR */}
          (Active users will appear here)
        </div>
      </div>
    </div>
  );
};

export default SignalRBookList;
