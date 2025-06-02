// BookUpdatesComponent.tsx
// Example React component using WebSockets for real-time book updates

import React, { useEffect, useState } from 'react';
import { useWebSocket } from './WebSocketClient';

// Define book type
interface Book {
  id: string;
  title: string;
  author: string;
  availability: boolean;
  lastUpdated: string;
}

// Status indicator component
const ConnectionStatus: React.FC<{ status: string }> = ({ status }) => {
  const getStatusColor = () => {
    switch (status) {
      case 'connected': return 'green';
      case 'connecting': return 'orange';
      case 'reconnecting': return 'orange';
      case 'disconnected': return 'red';
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

// Book list with real-time updates
const RealTimeBookList: React.FC = () => {
  // Initial books state
  const [books, setBooks] = useState<Book[]>([]);
  
  // Use our WebSocket hook
  const { 
    connectionState, 
    messages, 
    error, 
    sendMessage, 
    reconnect 
  } = useWebSocket('ws://localhost:5000/books-ws');
  
  // Subscribe to book updates when connected
  useEffect(() => {
    if (connectionState === 'connected') {
      sendMessage('subscribe', { channel: 'book-updates' });
    }
  }, [connectionState, sendMessage]);
  
  // Process incoming messages
  useEffect(() => {
    // Only process if we have messages
    if (messages.length > 0) {
      // Get the latest message
      const latestMessage = messages[messages.length - 1];
      
      switch (latestMessage.type) {
        case 'initial-data':
          // Initial book data
          setBooks(latestMessage.data.books);
          break;
          
        case 'book-added':
          // Add a new book
          setBooks(currentBooks => [...currentBooks, latestMessage.data.book]);
          break;
          
        case 'book-updated':
          // Update an existing book
          setBooks(currentBooks => 
            currentBooks.map(book => 
              book.id === latestMessage.data.book.id ? latestMessage.data.book : book
            )
          );
          break;
          
        case 'book-deleted':
          // Remove a book
          setBooks(currentBooks => 
            currentBooks.filter(book => book.id !== latestMessage.data.bookId)
          );
          break;
          
        default:
          console.log('Unknown message type:', latestMessage.type);
      }
    }
  }, [messages]);
  
  // Handle availability toggle with WebSocket
  const toggleAvailability = (bookId: string) => {
    const book = books.find(b => b.id === bookId);
    if (book) {
      sendMessage('update-book', {
        bookId,
        changes: { 
          availability: !book.availability,
          lastUpdated: new Date().toISOString()
        }
      });
    }
  };
  
  return (
    <div className="real-time-books">
      <h2>Real-Time Book Management</h2>
      
      {/* Display connection status */}
      <ConnectionStatus status={connectionState} />
      
      {/* Error message if any */}
      {error && <div className="error-message" style={{ color: 'red', marginBottom: '1rem' }}>{error}</div>}
      
      {/* Reconnect button */}
      {connectionState === 'disconnected' && (
        <button 
          onClick={reconnect}
          style={{ marginBottom: '1rem' }}
        >
          Reconnect
        </button>
      )}
      
      {/* Book list */}
      <div className="book-list">
        {books.length === 0 ? (
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
    </div>
  );
};

export default RealTimeBookList;
