// SignalRService.ts
// A service for connecting to SignalR hubs from React

import * as signalR from '@microsoft/signalr';
import { useState, useEffect, useCallback } from 'react';

// Types for our book operations
export interface Book {
  id: string;
  title: string;
  author: string;
  availability: boolean;
  lastUpdated: string;
}

export type BookOperation = 'added' | 'updated' | 'deleted';

export interface BookEvent {
  operation: BookOperation;
  book?: Book;
  bookId?: string;
}

// Connection states 
export type SignalRConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting' | 'error';

// SignalR service class for our book application
export class BookSignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private hubUrl: string;
  
  constructor(hubUrl: string) {
    this.hubUrl = hubUrl;
  }
  
  // Start the connection to the SignalR hub
  public async start(): Promise<boolean> {
    try {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(this.hubUrl)
        .withAutomaticReconnect([0, 2000, 5000, 10000, 20000]) // Reconnect policy
        .configureLogging(signalR.LogLevel.Information)
        .build();
        
      await this.hubConnection.start();
      console.log('SignalR connected successfully');
      return true;
    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      return false;
    }
  }
  
  // Stop the connection
  public async stop(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
    }
  }
  
  // Subscribe to book events
  public onBookEvent(callback: (event: BookEvent) => void): void {
    if (!this.hubConnection) {
      throw new Error('SignalR connection not started');
    }
    
    this.hubConnection.on('BookAdded', (book: Book) => {
      callback({ operation: 'added', book });
    });
    
    this.hubConnection.on('BookUpdated', (book: Book) => {
      callback({ operation: 'updated', book });
    });
    
    this.hubConnection.on('BookDeleted', (bookId: string) => {
      callback({ operation: 'deleted', bookId });
    });
  }
  
  // Subscribe to a specific book's updates
  public async subscribeToBook(bookId: string): Promise<void> {
    if (!this.hubConnection) {
      throw new Error('SignalR connection not started');
    }
    
    await this.hubConnection.invoke('SubscribeToBook', bookId);
  }
  
  // Unsubscribe from a book's updates
  public async unsubscribeFromBook(bookId: string): Promise<void> {
    if (!this.hubConnection) {
      throw new Error('SignalR connection not started');
    }
    
    await this.hubConnection.invoke('UnsubscribeFromBook', bookId);
  }
  
  // Update book availability via SignalR
  public async updateBookAvailability(bookId: string, isAvailable: boolean): Promise<void> {
    if (!this.hubConnection) {
      throw new Error('SignalR connection not started');
    }
    
    await this.hubConnection.invoke('UpdateBookAvailability', bookId, isAvailable);
  }
  
  // Get connection status
  public getConnectionState(): signalR.HubConnectionState {
    if (!this.hubConnection) {
      return signalR.HubConnectionState.Disconnected;
    }
    
    return this.hubConnection.state;
  }
}

// React hook to use SignalR in components
export const useSignalR = (hubUrl: string) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [connectionState, setConnectionState] = useState<SignalRConnectionState>('disconnected');
  const [error, setError] = useState<string | null>(null);
  
  // Start the connection
  const startConnection = useCallback(async () => {
    try {
      setConnectionState('connecting');
      setError(null);
      
      const newConnection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();
        
      // Set up connection state change handlers
      newConnection.onreconnecting(() => {
        setConnectionState('reconnecting');
      });
      
      newConnection.onreconnected(() => {
        setConnectionState('connected');
      });
      
      newConnection.onclose(() => {
        setConnectionState('disconnected');
      });
      
      await newConnection.start();
      setConnection(newConnection);
      setConnectionState('connected');
    } catch (err) {
      setConnectionState('error');
      setError(`Failed to connect: ${err}`);
      console.error('SignalR connection error:', err);
    }
  }, [hubUrl]);
  
  // Stop the connection
  const stopConnection = useCallback(async () => {
    if (connection) {
      await connection.stop();
      setConnection(null);
      setConnectionState('disconnected');
    }
  }, [connection]);
  
  // Register a handler for a specific method
  const on = useCallback((methodName: string, newMethod: (...args: any[]) => void) => {
    if (connection) {
      connection.on(methodName, newMethod);
    }
  }, [connection]);
  
  // Remove a handler for a specific method
  const off = useCallback((methodName: string, method: (...args: any[]) => void) => {
    if (connection) {
      connection.off(methodName, method);
    }
  }, [connection]);
  
  // Invoke a hub method
  const invoke = useCallback(async (methodName: string, ...args: any[]) => {
    if (connection) {
      try {
        return await connection.invoke(methodName, ...args);
      } catch (err) {
        console.error(`Error invoking '${methodName}':`, err);
        throw err;
      }
    }
    throw new Error('No connection available');
  }, [connection]);
  
  // Connect on component mount
  useEffect(() => {
    startConnection();
    
    // Clean up on unmount
    return () => {
      if (connection) {
        connection.stop();
      }
    };
  }, [hubUrl]); // Re-run if the hub URL changes
  
  return {
    connection,
    connectionState,
    error,
    startConnection,
    stopConnection,
    on,
    off,
    invoke
  };
};

export default BookSignalRService;
