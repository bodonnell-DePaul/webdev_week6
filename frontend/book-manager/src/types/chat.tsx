export interface ChatMessage {
  type: 'user' | 'bot' | 'error';
  message: string;
  timestamp: Date;
}

export interface ChatBotProps {
  isOpen: boolean;
  onToggle: () => void;
}