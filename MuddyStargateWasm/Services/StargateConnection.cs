using System.Text.Json.Nodes;

namespace MuddyStargateWasm.Services
{
    enum MessageType
    {
        CloseWormhole,
        None
    }

    public class StargateConnection(ILogger<StargateConnection> logger, WebSocketService WS) : IDisposable
    {
        private static Uri uri = new Uri("wss://gatesocket.ancientsofresonite.net/");
        ILogger<StargateConnection> logger = logger;

        private MessageType lastSentMessageType = MessageType.None;

        public bool IsWSConnected => WS.IsConnected;

        public bool IsWormholeOpen { get; private set; } = false; // This needs to be listened to by the UI so we can show the correct state of the connection

        public async Task FreshClient()
        {
            await WS.ConnectAsync(uri.ToString());

            WS.OnMessage += HandleMessage;
            WS.OnDisconnected += () =>
            {
                logger.LogWarning($"WebSocketService disconnected :(");
            };
        }

        private async void HandleMessage(string message)
        {
            logger.LogInformation($"Message received from WebSocketService: {message}");
            if (message == "200")
            {
                switch (lastSentMessageType)
                {
                    case MessageType.CloseWormhole:
                        IsWormholeOpen = false;
                        break;
                    case MessageType.None:
                        logger.LogWarning("Received a 200 but we had nothing waiting for a response. This should not happen");
                        break;
                    default:
                        break;
                }
            }
        }

        public async Task RequestAddress()
        {
            dynamic requestAddress = new JsonObject()
            {
                ["type"] = "requestAddress",
                ["gate_address"] = "159632", // The address of the gate we are creating 
                ["gate_code"] = "M@",
                ["host_id"] = "BasementWorlds",
                ["is_headless"] = true,
                ["session_id"] = "ressession:///S-019d3d07-fe69-7594-a35a-014674b5d535",
                ["current_users"] = 0,
                ["max_users"] = 0,
                ["public"] = false,
                ["gate_name"] = "A dirty test gate"
            };
            logger.LogInformation($"Requesting wormhole address...");
            await WS.SendAsync(requestAddress.ToString());
            logger.LogInformation($"Requested address!");
        }

        public async Task DialRequest(string address)
        {
            dynamic dialRequest = new JsonObject()
            {
                ["type"] = "dialRequest",
                ["gate_address"] = address
            };

            await WS.SendAsync(dialRequest.ToString());
            IsWormholeOpen = true;
            logger.LogInformation($"Requested to dial a gate");
        }

        public async Task CloseWormhole()
        {
            dynamic close = new JsonObject()
            {
                ["type"] = "closeWormhole"
            };

            lastSentMessageType = MessageType.CloseWormhole;
            logger.LogInformation($"Closing wormhole connection...");
            await WS.SendAsync(close.ToString());
        }

        public async void Dispose()
        {
            await CloseWormhole();
        }
    }
}
