// SSEClient.ts
// A client for Server-Sent Events (SSE) in React applications

import { useState, useEffect, useRef, useCallback } from 'react';

// Event types
export interface SSEEvent {
  id?: string;
  type: string;
  data: any;
  retry?: number;
}

// Connection states
export type SSEConnectionState = 'connecting' | 'connected' | 'disconnected' | 'error';

// Options for the SSE client
export interface SSEOptions {
  withCredentials?: boolean;
  retry?: number; // Reconnection time in milliseconds
  headers?: Record<string, string>;
}

// Hook for using Server-Sent Events in React components
export const useSSE = (url: string, options: SSEOptions = {}) => {
  // State for connection status and events
  const [connectionState, setConnectionState] = useState<SSEConnectionState>('disconnected');
  const [lastEvent, setLastEvent] = useState<SSEEvent | null>(null);
  const [events, setEvents] = useState<SSEEvent[]>([]);
  const [error, setError] = useState<string | null>(null);
  
  // Use a ref to maintain the EventSource instance across renders
  const eventSourceRef = useRef<EventSource | null>(null);
  const retryTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  
  // Connect to the SSE endpoint
  const connect = useCallback(() => {
    // Clean up existing connection
    if (eventSourceRef.current) {
      eventSourceRef.current.close();
    }
    
    if (retryTimeoutRef.current) {
      clearTimeout(retryTimeoutRef.current);
    }
    
    try {
      setConnectionState('connecting');
      
      // Create new EventSource
      const eventSource = new EventSource(url, {
        withCredentials: options.withCredentials
      });
      
      // Set up event handlers
      eventSource.onopen = () => {
        setConnectionState('connected');
        setError(null);
      };
      
      eventSource.onerror = (event) => {
        // Handle errors
        console.error('SSE error:', event);
        
        // If the connection is closed, attempt to reconnect
        if (eventSource.readyState === EventSource.CLOSED) {
          setConnectionState('disconnected');
          handleReconnect();
        } else {
          setError('Error in SSE connection');
          setConnectionState('error');
        }
      };
      
      // Handle message events (default event type)
      eventSource.onmessage = (event) => {
        try {
          const parsed = JSON.parse(event.data);
          const sseEvent: SSEEvent = {
            id: event.lastEventId,
            type: 'message',
            data: parsed
          };
          
          setLastEvent(sseEvent);
          setEvents(prev => [...prev, sseEvent]);
        } catch (err) {
          console.error('Error parsing SSE event data:', err);
          setError(`Error parsing event data: ${err}`);
        }
      };
      
      eventSourceRef.current = eventSource;
    } catch (err) {
      setConnectionState('error');
      setError(`Failed to connect to SSE endpoint: ${err}`);
      handleReconnect();
    }
  }, [url, options.withCredentials]);
  
  // Handle reconnection
  const handleReconnect = useCallback(() => {
    const retryTime = options.retry || 3000;
    
    if (retryTimeoutRef.current) {
      clearTimeout(retryTimeoutRef.current);
    }
    
    retryTimeoutRef.current = setTimeout(() => {
      connect();
    }, retryTime);
  }, [connect, options.retry]);
  
  // Add a listener for a specific event type
  const addEventTypeListener = useCallback((eventType: string, callback: (data: any) => void) => {
    if (!eventSourceRef.current) {
      return () => {};
    }
    
    const handler = (event: MessageEvent) => {
      try {
        const parsed = JSON.parse(event.data);
        callback(parsed);
      } catch (err) {
        console.error(`Error parsing SSE event (${eventType}):`, err);
      }
    };
    
    eventSourceRef.current.addEventListener(eventType, handler);
    
    return () => {
      if (eventSourceRef.current) {
        eventSourceRef.current.removeEventListener(eventType, handler);
      }
    };
  }, []);
  
  // Connect on component mount
  useEffect(() => {
    connect();
    
    // Cleanup on unmount
    return () => {
      if (retryTimeoutRef.current) {
        clearTimeout(retryTimeoutRef.current);
      }
      
      if (eventSourceRef.current) {
        eventSourceRef.current.close();
      }
    };
  }, [connect]);
  
  // Return the hook API
  return {
    connectionState,
    lastEvent,
    events,
    error,
    connect,
    addEventListener: addEventTypeListener
  };
};

// More specialized class for book SSE
export class BookSSEClient {
  private eventSource: EventSource | null = null;
  private url: string;
  private handlers: Map<string, ((data: any) => void)[]> = new Map();
  private connectionStateHandler: ((state: SSEConnectionState) => void) | null = null;
  
  constructor(url: string) {
    this.url = url;
  }
  
  // Start listening for events
  public connect(): void {
    if (this.eventSource) {
      this.eventSource.close();
    }
    
    try {
      this.notifyConnectionState('connecting');
      
      this.eventSource = new EventSource(this.url);
      
      // Handle connection open
      this.eventSource.onopen = () => {
        this.notifyConnectionState('connected');
      };
      
      // Handle errors
      this.eventSource.onerror = () => {
        if (this.eventSource?.readyState === EventSource.CLOSED) {
          this.notifyConnectionState('disconnected');
        } else {
          this.notifyConnectionState('error');
        }
      };
      
      // Set up default message handler
      this.eventSource.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data);
          
          // Call all registered 'message' handlers
          this.notifyHandlers('message', data);
        } catch (error) {
          console.error('Error parsing SSE message:', error);
        }
      };
      
      // Add handlers for specific event types
      
      // Book added
      this.eventSource.addEventListener('book-added', (event) => {
        try {
          const data = JSON.parse(event.data);
          this.notifyHandlers('book-added', data);
        } catch (error) {
          console.error('Error parsing book-added event:', error);
        }
      });
      
      // Book updated
      this.eventSource.addEventListener('book-updated', (event) => {
        try {
          const data = JSON.parse(event.data);
          this.notifyHandlers('book-updated', data);
        } catch (error) {
          console.error('Error parsing book-updated event:', error);
        }
      });
      
      // Book deleted
      this.eventSource.addEventListener('book-deleted', (event) => {
        try {
          const data = JSON.parse(event.data);
          this.notifyHandlers('book-deleted', data);
        } catch (error) {
          console.error('Error parsing book-deleted event:', error);
        }
      });
      
    } catch (error) {
      console.error('Error setting up SSE connection:', error);
      this.notifyConnectionState('error');
    }
  }
  
  // Register a handler for connection state changes
  public onConnectionStateChange(handler: (state: SSEConnectionState) => void): void {
    this.connectionStateHandler = handler;
  }
  
  // Register a handler for a specific event type
  public on(eventType: string, handler: (data: any) => void): void {
    if (!this.handlers.has(eventType)) {
      this.handlers.set(eventType, []);
    }
    
    this.handlers.get(eventType)?.push(handler);
  }
  
  // Unregister a handler
  public off(eventType: string, handler: (data: any) => void): void {
    if (!this.handlers.has(eventType)) {
      return;
    }
    
    const handlers = this.handlers.get(eventType);
    if (handlers) {
      const index = handlers.indexOf(handler);
      if (index !== -1) {
        handlers.splice(index, 1);
      }
    }
  }
  
  // Notify all registered handlers for an event type
  private notifyHandlers(eventType: string, data: any): void {
    const handlers = this.handlers.get(eventType);
    if (handlers) {
      handlers.forEach(handler => {
        try {
          handler(data);
        } catch (error) {
          console.error(`Error in SSE handler for ${eventType}:`, error);
        }
      });
    }
  }
  
  // Notify the connection state handler
  private notifyConnectionState(state: SSEConnectionState): void {
    if (this.connectionStateHandler) {
      try {
        this.connectionStateHandler(state);
      } catch (error) {
        console.error('Error in SSE connection state handler:', error);
      }
    }
  }
  
  // Close the connection
  public disconnect(): void {
    if (this.eventSource) {
      this.eventSource.close();
      this.eventSource = null;
      this.notifyConnectionState('disconnected');
    }
  }
}
