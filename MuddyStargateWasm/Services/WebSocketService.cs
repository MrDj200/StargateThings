using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;

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
            Logger.LogDebug($"Attempting to connect to WebSocket at {url}...");
            _ws = new ClientWebSocket();
            _cts = new CancellationTokenSource();

            await _ws.ConnectAsync(new Uri(url), _cts.Token);

            _ = ReceiveLoop(); // fire and forget
            _ = DisconnectionLoop();
            _ = KeepAliveLoop();
        }

        public async Task SendAsync(string message)
        {
            if (!IsConnected) return;

            Logger.LogDebug($"Sending message: {message}");

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

            Logger.LogDebug("RECEIVING STOPPED!");
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

        public event Action? OnKeepAlive;
        private async Task KeepAliveLoop()
        {
            try
            {
                while (_ws.State == WebSocketState.Open)
                {
                    await Task.Delay(TimeSpan.FromSeconds(60), _cts.Token);

                    if (_ws.State != WebSocketState.Open)
                        break;

                    dynamic keepAliveMessage = new JsonObject()
                    {
                        ["type"] = "keepAlive"
                    };

                    await this.SendAsync(keepAliveMessage.ToString());

                    OnKeepAlive?.Invoke();
                    Logger.LogDebug("Keepalive sent");
                }
            }
            catch (TaskCanceledException)
            {
                // normal on disconnect
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "KeepAlive loop crashed");
                await this.DisconnectAsync();
            }
        }
    }
}
