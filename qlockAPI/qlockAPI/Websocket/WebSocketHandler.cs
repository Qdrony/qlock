using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace qlockAPI.Websocket
{
    public class WebSocketHandler
    {
        private readonly ConcurrentDictionary<int, WebSocket> _connections = new();

        public void AddConnection(int userId, WebSocket webSocket)
        {
            _connections[userId] = webSocket;
        }

        public WebSocket GetConnection(int userId)
        {
            return _connections.ContainsKey(userId) ? _connections[userId] : null!;
        }

        public void RemoveConnection(int userId)
        {
            _connections.TryRemove(userId, out _);
        }

        public bool HasActiveConnection(int userId)
        {
            return _connections.ContainsKey(userId);
        }

        public async Task SendMessageAsync(int userId, object message)
        {
            if (_connections.TryGetValue(userId, out var webSocket) && webSocket.State == WebSocketState.Open)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(message);
                var buffer = Encoding.UTF8.GetBytes(json);
                var segment = new ArraySegment<byte>(buffer);
                await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var userMessage = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
            int userId = (int)userMessage["userId"];

            if(!HasActiveConnection(userId))
            {
                AddConnection(userId,webSocket);
            }

            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }

}
