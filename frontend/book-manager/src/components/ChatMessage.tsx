import React from 'react';
import { ChatMessage as ChatMessageType } from '../types/chat';

interface ChatMessageProps {
  message: ChatMessageType;
}

export const ChatMessage: React.FC<ChatMessageProps> = ({ message }) => {
  const isBot = message.type === 'bot';
  const isError = message.type === 'error';

  return (
    <div className={`flex ${isBot ? 'justify-start' : 'justify-end'} mb-4`}>
      <div
        className={`max-w-xs lg:max-w-md px-4 py-2 rounded-lg ${
          isError
            ? 'bg-red-100 text-red-800'
            : isBot
            ? 'bg-gray-200 text-gray-800'
            : 'bg-blue-500 text-white'
        }`}
        style={{
          maxWidth: '70%',
          padding: '8px 12px',
          borderRadius: '8px',
          backgroundColor: isError 
            ? '#fee2e2' 
            : isBot 
            ? '#f3f4f6' 
            : '#3b82f6',
          color: isError 
            ? '#991b1b' 
            : isBot 
            ? '#374151' 
            : 'white'
        }}
      >
        <p style={{ 
          margin: 0, 
          fontSize: '14px', 
          whiteSpace: 'pre-line' 
        }}>
          {message.message}
        </p>
        <p style={{ 
          margin: '4px 0 0 0', 
          fontSize: '12px', 
          opacity: 0.7 
        }}>
          {message.timestamp.toLocaleTimeString()}
        </p>
      </div>
    </div>
  );
};