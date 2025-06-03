import { useState, useEffect, useRef, useCallback } from 'react';
import { ChatMessage } from '../types/chat';

export const useWebSocket = (url: string) => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [isConnected, setIsConnected] = useState(false);
  const [isConnecting, setIsConnecting] = useState(false);
  const wsRef = useRef<WebSocket | null>(null);

  const connect = useCallback(() => {
    if (wsRef.current?.readyState === WebSocket.OPEN) return;
    
    setIsConnecting(true);
    wsRef.current = new WebSocket(url);

    wsRef.current.onopen = () => {
      setIsConnected(true);
      setIsConnecting(false);
    };

    wsRef.current.onmessage = (event) => {
      try {
        const message: ChatMessage = JSON.parse(event.data);
        message.timestamp = new Date(message.timestamp);
        setMessages(prev => [...prev, message]);
      } catch (error) {
        console.error('Error parsing message:', error);
      }
    };

    wsRef.current.onclose = () => {
      setIsConnected(false);
      setIsConnecting(false);
    };

    wsRef.current.onerror = (error) => {
      console.error('WebSocket error:', error);
      setIsConnecting(false);
    };
  }, [url]);

  const disconnect = useCallback(() => {
    if (wsRef.current) {
      wsRef.current.close();
      wsRef.current = null;
    }
  }, []);

  const sendMessage = useCallback((message: string) => {
    if (wsRef.current?.readyState === WebSocket.OPEN) {
      const chatMessage: ChatMessage = {
        type: 'user',
        message,
        timestamp: new Date()
      };
      
      setMessages(prev => [...prev, chatMessage]);
      wsRef.current.send(JSON.stringify(chatMessage));
    }
  }, []);

  useEffect(() => {
    return () => {
      disconnect();
    };
  }, [disconnect]);

  return {
    messages,
    isConnected,
    isConnecting,
    connect,
    disconnect,
    sendMessage
  };
};