using System.Net.WebSockets;
using System.Text;

namespace MuddyStargateWasm.Services
{
    public class WebSocketService(ILogger<WebSocketService> Logger)
    {
        ILogger<WebSocketService> Logger = Logger;

        private ClientWebSocket _ws = new();
        private CancellationTokenSource _cts = new();

        public bool IsConnected => _ws.State == WebSocketState.Open;

        public async Task ConnectAsync(string url)
        {
            Logger.LogInformation($"Attempting to connect to WebSocket at {url}...");
            _ws = new ClientWebSocket();
            _cts = new CancellationTokenSource();

            await _ws.ConnectAsync(new Uri(url), _cts.Token);

            _ = ReceiveLoop(); // fire and forget
            _ = DisconnectionLoop();
        }

        public async Task SendAsync(string message)
        {
            if (!IsConnected) return;

            Logger.LogInformation($"Sending message: {message}");

            var bytes = Encoding.UTF8.GetBytes(message);
            await _ws.SendAsync(bytes, WebSocketMessageType.Text, true, _cts.Token);
        }

        public async Task DisconnectAsync()
        {
            if (_ws.State == WebSocketState.Open)
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", _cts.Token);
            }
        }

        public event Action<string>? OnMessage;
        private async Task ReceiveLoop()
        {
            var buffer = new byte[4096];

            while (_ws.State == WebSocketState.Open)
            {
                var result = await _ws.ReceiveAsync(buffer, _cts.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await DisconnectAsync();
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                OnMessage?.Invoke(message);
            }

            Logger.LogWarning("RECEIVING STOPPED!");
        }

        public event Action? OnDisconnected;
        private async Task DisconnectionLoop()
        {
            while (_ws.State == WebSocketState.Open)
            {
                await Task.Delay(1000);
            }
            Logger.LogWarning("Websocket disconnected!");
            OnDisconnected?.Invoke();
        }
    }
}
