// WebSocketClient.ts
// A reusable WebSocket client for React applications

import { useEffect, useState, useRef, useCallback } from 'react';

// Define message types
export interface WebSocketMessage {
  type: string;
  data: any;
}

// Connection states
export type ConnectionState = 'connecting' | 'connected' | 'disconnected' | 'reconnecting';

// Hook for using WebSockets in React components
export const useWebSocket = (url: string) => {
  // State for connection status and messages
  const [connectionState, setConnectionState] = useState<ConnectionState>('disconnected');
  const [messages, setMessages] = useState<WebSocketMessage[]>([]);
  const [error, setError] = useState<string | null>(null);
  
  // Use a ref to maintain the WebSocket instance across renders
  const webSocketRef = useRef<WebSocket | null>(null);
  
  // Reconnection settings
  const reconnectAttempts = useRef(0);
  const maxReconnectAttempts = 5;
  const reconnectTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  
  // Connect to WebSocket
  const connect = useCallback(() => {
    // Close any existing connection
    if (webSocketRef.current) {
      webSocketRef.current.close();
    }
    
    try {
      setConnectionState('connecting');
      
      // Create new WebSocket connection
      const ws = new WebSocket(url);
      
      // Setup event handlers
      ws.onopen = () => {
        setConnectionState('connected');
        setError(null);
        reconnectAttempts.current = 0;
      };
      
      ws.onmessage = (event) => {
        try {
          const message = JSON.parse(event.data) as WebSocketMessage;
          setMessages((prevMessages) => [...prevMessages, message]);
        } catch (err) {
          console.error('Error parsing WebSocket message:', err);
        }
      };
      
      ws.onerror = (event) => {
        setError('WebSocket error occurred');
        console.error('WebSocket error:', event);
      };
      
      ws.onclose = (event) => {
        setConnectionState('disconnected');
        
        // Attempt to reconnect if the connection was not closed cleanly
        if (event.code !== 1000) {
          handleReconnect();
        }
      };
      
      webSocketRef.current = ws;
    } catch (err) {
      setError(`Failed to connect: ${err}`);
      setConnectionState('disconnected');
      handleReconnect();
    }
  }, [url]);
  
  // Handle reconnection logic
  const handleReconnect = useCallback(() => {
    if (reconnectAttempts.current >= maxReconnectAttempts) {
      setError(`Maximum reconnection attempts (${maxReconnectAttempts}) reached.`);
      return;
    }
    
    if (reconnectTimeoutRef.current) {
      clearTimeout(reconnectTimeoutRef.current);
    }
    
    // Exponential backoff for reconnection
    const timeout = Math.min(1000 * (2 ** reconnectAttempts.current), 30000);
    reconnectAttempts.current += 1;
    
    setConnectionState('reconnecting');
    reconnectTimeoutRef.current = setTimeout(() => {
      connect();
    }, timeout);
  }, [connect]);
  
  // Send a message through the WebSocket
  const sendMessage = useCallback((type: string, data: any) => {
    if (webSocketRef.current && webSocketRef.current.readyState === WebSocket.OPEN) {
      const message: WebSocketMessage = { type, data };
      webSocketRef.current.send(JSON.stringify(message));
      return true;
    }
    return false;
  }, []);
  
  // Connect on component mount
  useEffect(() => {
    connect();
    
    // Cleanup on component unmount
    return () => {
      if (reconnectTimeoutRef.current) {
        clearTimeout(reconnectTimeoutRef.current);
      }
      
      if (webSocketRef.current) {
        webSocketRef.current.close();
      }
    };
  }, [connect]);
  
  return {
    connectionState,
    messages,
    error,
    sendMessage,
    reconnect: connect,
  };
};

// Example of a more specific WebSocket client for books
export class BookWebSocketClient {
  private ws: WebSocket | null = null;
  private url: string;
  private onMessageCallback: (data: any) => void;
  private onStatusChangeCallback: (status: ConnectionState) => void;
  
  constructor(
    url: string,
    onMessage: (data: any) => void,
    onStatusChange: (status: ConnectionState) => void
  ) {
    this.url = url;
    this.onMessageCallback = onMessage;
    this.onStatusChangeCallback = onStatusChange;
  }
  
  // Connect to the WebSocket server
  public connect(): void {
    if (this.ws) {
      this.ws.close();
    }
    
    this.onStatusChangeCallback('connecting');
    
    this.ws = new WebSocket(this.url);
    
    this.ws.onopen = () => {
      this.onStatusChangeCallback('connected');
    };
    
    this.ws.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        this.onMessageCallback(data);
      } catch (error) {
        console.error('Error parsing WebSocket message:', error);
      }
    };
    
    this.ws.onclose = () => {
      this.onStatusChangeCallback('disconnected');
    };
    
    this.ws.onerror = (error) => {
      console.error('WebSocket error:', error);
      this.onStatusChangeCallback('disconnected');
    };
  }
  
  // Send a message to the server
  public sendMessage(message: any): boolean {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(JSON.stringify(message));
      return true;
    }
    return false;
  }
  
  // Subscribe to book updates
  public subscribeToBookUpdates(): void {
    this.sendMessage({
      type: 'subscribe',
      channel: 'book-updates'
    });
  }
  
  // Subscribe to specific book
  public subscribeToBook(bookId: string): void {
    this.sendMessage({
      type: 'subscribe',
      channel: 'book',
      bookId
    });
  }
  
  // Close the connection
  public disconnect(): void {
    if (this.ws) {
      this.ws.close();
      this.ws = null;
    }
  }
}
