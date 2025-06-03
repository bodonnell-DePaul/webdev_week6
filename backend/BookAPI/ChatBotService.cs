using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace BookAPI.Services
{
    public class ChatBotService
    {
        private readonly ILogger<ChatBotService> _logger;

        public ChatBotService(ILogger<ChatBotService> logger)
        {
            _logger = logger;
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            try
            {
                // Send welcome message
                await SendMessageAsync(webSocket, new ChatMessage
                {
                    Type = "bot",
                    Message = "Hello! I'm your book assistant. How can I help you today?",
                    Timestamp = DateTime.UtcNow
                });

                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), 
                        CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await ProcessUserMessage(webSocket, message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Connection closed by client",
                            CancellationToken.None);
                    }
                }
            }
            catch (WebSocketException ex)
            {
                _logger.LogError(ex, "WebSocket error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in WebSocket handler");
            }
        }

        private async Task ProcessUserMessage(WebSocket webSocket, string userMessage)
        {
            try
            {
                var chatMessage = JsonSerializer.Deserialize<ChatMessage>(userMessage);
                if (chatMessage != null)
                {
                    // Simple bot response logic
                    var botResponse = GenerateBotResponse(chatMessage.Message);
                    
                    await SendMessageAsync(webSocket, new ChatMessage
                    {
                        Type = "bot",
                        Message = botResponse,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (JsonException)
            {
                await SendMessageAsync(webSocket, new ChatMessage
                {
                    Type = "error",
                    Message = "Invalid message format",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        private string GenerateBotResponse(string userMessage)
        {
            var message = userMessage.ToLower();

            if (message.Contains("book") && message.Contains("recommend"))
            {
                return "I'd recommend checking out our fiction section! We have great titles in mystery, romance, and sci-fi genres.";
            }
            else if (message.Contains("available") || message.Contains("stock"))
            {
                return "You can check book availability using our search feature. What specific book are you looking for?";
            }
            else if (message.Contains("help"))
            {
                return "I can help you with:\n• Book recommendations\n• Checking availability\n• Finding books by genre or author\n• General library information";
            }
            else if (message.Contains("hello") || message.Contains("hi"))
            {
                return "Hello! Welcome to our book library. What can I help you find today?";
            }
            else if (message.Contains("genre"))
            {
                return "We have books in many genres including Fiction, Non-Fiction, Mystery, Romance, Sci-Fi, Biography, and more. What genre interests you?";
            }
            else
            {
                return "I'm here to help with book-related questions! You can ask me about book recommendations, availability, or browse by genre.";
            }
        }

        private async Task SendMessageAsync(WebSocket webSocket, ChatMessage message)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }
    }

    public class ChatMessage
    {
        public string Type { get; set; } = string.Empty; // "user", "bot", "error"
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}