import React, { useState, useRef, useEffect } from 'react';
import { useWebSocket } from '../services/useWebSockets';
import { ChatMessage } from './ChatMessage';
import { ChatBotProps } from '../types/chat';

export const ChatBot: React.FC<ChatBotProps> = ({ isOpen, onToggle }) => {
  const [inputMessage, setInputMessage] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  
  // Adjust the WebSocket URL to match your backend port
  const WS_URL = 'ws://localhost:5137/ws/chat';
  const { messages, isConnected, isConnecting, connect, disconnect, sendMessage } = useWebSocket(WS_URL);

  useEffect(() => {
    if (isOpen && !isConnected && !isConnecting) {
      connect();
    } else if (!isOpen && isConnected) {
      disconnect();
    }
  }, [isOpen, isConnected, isConnecting, connect, disconnect]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (inputMessage.trim() && isConnected) {
      sendMessage(inputMessage.trim());
      setInputMessage('');
    }
  };

  if (!isOpen) {
    return (
      <button
        onClick={onToggle}
        style={{
          position: 'fixed',
          bottom: '20px',
          right: '20px',
          backgroundColor: '#3b82f6',
          color: 'white',
          border: 'none',
          borderRadius: '50%',
          width: '60px',
          height: '60px',
          fontSize: '24px',
          cursor: 'pointer',
          boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
          transition: 'background-color 0.2s'
        }}
        onMouseOver={(e) => e.currentTarget.style.backgroundColor = '#2563eb'}
        onMouseOut={(e) => e.currentTarget.style.backgroundColor = '#3b82f6'}
        aria-label="Open chat"
      >
        💬
      </button>
    );
  }

  return (
    <div style={{
      position: 'fixed',
      bottom: '20px',
      right: '20px',
      width: '320px',
      height: '400px',
      backgroundColor: 'white',
      border: '1px solid #d1d5db',
      borderRadius: '8px',
      boxShadow: '0 10px 25px rgba(0, 0, 0, 0.15)',
      display: 'flex',
      flexDirection: 'column',
      zIndex: 1000
    }}>
      {/* Header */}
      <div style={{
        backgroundColor: '#3b82f6',
        color: 'white',
        padding: '12px',
        borderTopLeftRadius: '8px',
        borderTopRightRadius: '8px',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center'
      }}>
        <h3 style={{ margin: 0, fontSize: '16px', fontWeight: '600' }}>
          Book Assistant
        </h3>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <div style={{
            width: '8px',
            height: '8px',
            borderRadius: '50%',
            backgroundColor: isConnected ? '#10b981' : '#ef4444'
          }} />
          <button
            onClick={onToggle}
            style={{
              background: 'none',
              border: 'none',
              color: 'white',
              fontSize: '18px',
              cursor: 'pointer'
            }}
            aria-label="Close chat"
          >
            ✕
          </button>
        </div>
      </div>

      {/* Messages */}
      <div style={{
        flex: 1,
        overflowY: 'auto',
        padding: '12px',
        display: 'flex',
        flexDirection: 'column',
        gap: '8px'
      }}>
        {isConnecting && (
          <div style={{
            textAlign: 'center',
            color: '#6b7280',
            fontSize: '14px'
          }}>
            Connecting to chat...
          </div>
        )}
        
        {!isConnected && !isConnecting && (
          <div style={{
            textAlign: 'center',
            color: '#6b7280',
            fontSize: '14px'
          }}>
            <p style={{ margin: '0 0 8px 0' }}>Disconnected from chat</p>
            <button 
              onClick={connect}
              style={{
                color: '#3b82f6',
                background: 'none',
                border: 'none',
                textDecoration: 'underline',
                cursor: 'pointer',
                fontSize: '14px'
              }}
            >
              Reconnect
            </button>
          </div>
        )}

        {messages.map((message, index) => (
          <ChatMessage key={index} message={message} />
        ))}
        <div ref={messagesEndRef} />
      </div>

      {/* Input */}
      <form onSubmit={handleSubmit} style={{
        padding: '12px',
        borderTop: '1px solid #e5e7eb'
      }}>
        <div style={{ display: 'flex', gap: '8px' }}>
          <input
            type="text"
            value={inputMessage}
            onChange={(e) => setInputMessage(e.target.value)}
            placeholder="Ask about books..."
            style={{
              flex: 1,
              padding: '8px 12px',
              border: '1px solid #d1d5db',
              borderRadius: '6px',
              fontSize: '14px',
              outline: 'none'
            }}
            onFocus={(e) => e.target.style.borderColor = '#3b82f6'}
            onBlur={(e) => e.target.style.borderColor = '#d1d5db'}
            disabled={!isConnected}
          />
          <button
            type="submit"
            disabled={!isConnected || !inputMessage.trim()}
            style={{
              backgroundColor: (!isConnected || !inputMessage.trim()) ? '#d1d5db' : '#3b82f6',
              color: (!isConnected || !inputMessage.trim()) ? '#9ca3af' : 'white',
              border: 'none',
              padding: '8px 16px',
              borderRadius: '6px',
              fontSize: '14px',
              cursor: (!isConnected || !inputMessage.trim()) ? 'not-allowed' : 'pointer',
              transition: 'background-color 0.2s'
            }}
          >
            Send
          </button>
        </div>
      </form>
    </div>
  );
};