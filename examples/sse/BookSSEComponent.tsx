// BookSSEComponent.tsx
// Example React component using Server-Sent Events for real-time book updates

import React, { useEffect, useState, useCallback } from 'react';
import { useSSE, SSEConnectionState } from './SSEClient';

// Define book type
interface Book {
  id: string;
  title: string;
  author: string;
  availability: boolean;
  lastUpdated: string;
}

// Status indicator component
const ConnectionStatus: React.FC<{ status: SSEConnectionState }> = ({ status }) => {
  const getStatusColor = () => {
    switch (status) {
      case 'connected': return 'green';
      case 'connecting': return 'orange';
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
      <span>Status: {status}</span>
    </div>
  );
};

// Book list with real-time updates via SSE
const SSEBookList: React.FC = () => {
  // Books state
  const [books, setBooks] = useState<Book[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  
  // Use our SSE hook
  const { 
    connectionState, 
    events, 
    error, 
    connect, 
    addEventListener 
  } = useSSE('https://localhost:5001/api/books/events');
  
  // Fetch initial book data
  const fetchBooks = useCallback(async () => {
    try {
      setIsLoading(true);
      const response = await fetch('https://localhost:5001/api/books');
      if (!response.ok) {
        throw new Error(`HTTP error: ${response.status}`);
      }
      const data = await response.json();
      setBooks(data);
    } catch (err) {
      console.error('Error fetching books:', err);
    } finally {
      setIsLoading(false);
    }
  }, []);
  
  // Set up event handlers for specific SSE event types
  useEffect(() => {
    // Clean up function for event listeners
    const cleanupFns: Array<() => void> = [];
    
    if (connectionState === 'connected') {
      // Handle book added events
      const bookAddedCleanup = addEventListener('book-added', (data) => {
        setBooks(prev => [...prev, data.book]);
      });
      cleanupFns.push(bookAddedCleanup);
      
      // Handle book updated events
      const bookUpdatedCleanup = addEventListener('book-updated', (data) => {
        setBooks(prev => 
          prev.map(book => 
            book.id === data.book.id ? data.book : book
          )
        );
      });
      cleanupFns.push(bookUpdatedCleanup);
      
      // Handle book deleted events
      const bookDeletedCleanup = addEventListener('book-deleted', (data) => {
        setBooks(prev => 
          prev.filter(book => book.id !== data.bookId)
        );
      });
      cleanupFns.push(bookDeletedCleanup);
    }
    
    // Clean up event listeners on unmount or when connection state changes
    return () => {
      cleanupFns.forEach(cleanup => cleanup());
    };
  }, [connectionState, addEventListener]);
  
  // Load initial books when the component mounts or connection is established
  useEffect(() => {
    if (connectionState === 'connected') {
      fetchBooks();
    }
  }, [connectionState, fetchBooks]);
  
  // Handle toggling book availability
  const toggleAvailability = async (bookId: string) => {
    const book = books.find(b => b.id === bookId);
    if (book) {
      try {
        const response = await fetch(`https://localhost:5001/api/books/${bookId}/availability`, {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({ isAvailable: !book.availability })
        });
        
        if (!response.ok) {
          throw new Error(`HTTP error: ${response.status}`);
        }
        
        // No need to update the state manually - we'll get an SSE event
        console.log('Availability update request sent');
      } catch (err) {
        console.error('Error updating availability:', err);
      }
    }
  };
  
  return (
    <div className="sse-books">
      <h2>Server-Sent Events Real-Time Book Management</h2>
      
      {/* Display connection status */}
      <ConnectionStatus status={connectionState} />
      
      {/* Error message if any */}
      {error && <div className="error-message" style={{ color: 'red', marginBottom: '1rem' }}>{error}</div>}
      
      {/* Reconnect button */}
      {(connectionState === 'disconnected' || connectionState === 'error') && (
        <button 
          onClick={connect}
          style={{ marginBottom: '1rem' }}
        >
          Reconnect
        </button>
      )}
      
      {/* Event count */}
      <div style={{ marginBottom: '1rem' }}>
        Total events received: {events.length}
      </div>
      
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
      
      {/* Last 5 events log */}
      <div className="events-log" style={{ marginTop: '2rem' }}>
        <h3>Recent Events</h3>
        <div style={{ 
          maxHeight: '200px', 
          overflowY: 'auto', 
          padding: '1rem',
          backgroundColor: '#f8f9fa',
          borderRadius: '5px'
        }}>
          {events.length === 0 ? (
            <p>No events received yet</p>
          ) : (
            events.slice(-5).map((event, index) => (
              <div key={index} style={{ marginBottom: '0.5rem', fontFamily: 'monospace' }}>
                <strong>{event.type}</strong>: {JSON.stringify(event.data)}
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
};

export default SSEBookList;
