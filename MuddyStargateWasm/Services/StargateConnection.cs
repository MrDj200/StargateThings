using System.Text.Json.Nodes;

namespace MuddyStargateWasm.Services
{
    enum MessageType
    {
        CloseWormhole,
        DialRequest,
        None
    }

    public class StargateConnection(ILogger<StargateConnection> logger, WebSocketService WS) : IDisposable
    {
        private static Uri uri = new Uri("wss://gatesocket.ancientsofresonite.net/");
        ILogger<StargateConnection> logger = logger;

        private MessageType lastSentMessageType = MessageType.None;

        public bool IsWSConnected => WS.IsConnected;

        private bool isWormholeOpen = false;
        public bool IsWormholeOpen 
        { 
            get => isWormholeOpen;
            private set
            {
                if (isWormholeOpen != value)
                {
                    isWormholeOpen = value;
                    OnChange?.Invoke();
                }
            }
        }

        public event Action? OnChange;

        public async Task FreshClient()
        {
            await WS.ConnectAsync(uri.ToString());

            WS.OnMessage += HandleMessage;
            WS.OnDisconnected += () =>
            {
                IsWormholeOpen = false;
                OnChange?.Invoke();
                logger.LogWarning($"WebSocketService disconnected :(");
            };
        }

        private async void HandleMessage(string message)
        {
            logger.LogDebug($"Message received from WebSocketService: {message}");
            if (message == "200")
            {
                switch (lastSentMessageType)
                {
                    case MessageType.CloseWormhole:
                        IsWormholeOpen = false;
                        lastSentMessageType = MessageType.None;
                        break;
                    case MessageType.None:
                        logger.LogWarning("Received a 200 but we had nothing waiting for a response. This should not happen");
                        break;
                    default:
                        break;
                }
            }
            else if (message.StartsWith("CS"))
            {
                var split = message.Split(':');

                switch (split[0])
                {
                    case "CSDialCheck":
                        if (split[1] == "200" && lastSentMessageType == MessageType.DialRequest)
                        {
                            IsWormholeOpen = true;
                            lastSentMessageType = MessageType.None;
                        }else if (split[1] == "403" && lastSentMessageType == MessageType.DialRequest)
                        {
                            isWormholeOpen = false;
                            lastSentMessageType = MessageType.None;
                        }
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
            lastSentMessageType = MessageType.DialRequest;
            logger.LogInformation($"Requested to dial gate {address}");
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
