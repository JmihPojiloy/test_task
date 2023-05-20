using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestWPF
{
    /// <summary>
    /// Класс с логикой подключения к серверу одометра
    /// </summary>
    public class ConnectionManager
    {
        private ClientWebSocket webSocket;
        private CancellationTokenSource cancellationTokenSource;
        private bool isConnected;
        private string url;

        public event EventHandler<bool> ConnectionStatusChanged;
        public event EventHandler<float> OdometerUpdated;

        public bool IsConnected => isConnected;
       
        public ConnectionManager(string url)
        {
            webSocket = new ClientWebSocket();
            cancellationTokenSource = new CancellationTokenSource();
            this.url = url;
        }

        public async Task Connect()
        {
            try
            {
                await webSocket.ConnectAsync(new Uri(this.url), cancellationTokenSource.Token);
                isConnected = true;
                ConnectionStatusChanged?.Invoke(this, true);
                await ReceiveMessagesAsync();
            }
            catch
            {
                isConnected = false;
                ConnectionStatusChanged?.Invoke(this, false);
            }
        }

        public async Task RequestOdometer()
        {
            if (IsConnected)
            {
                string request = "{\"operation\": \"getCurrentOdometer\"}";
                await SendMessageAsync(request);
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            while (IsConnected)
            {
                try
                {
                    var buffer = new ArraySegment<byte>(new byte[4096]);
                    var receivedResult = await webSocket.ReceiveAsync(buffer, cancellationTokenSource.Token);

                    if (receivedResult.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer.Array, 0, receivedResult.Count);
                        await ProcessMessageAsync(message);
                    }
                }
                catch
                {
                    isConnected = false;
                    ConnectionStatusChanged?.Invoke(this, false);
                    break;
                }
            }
        }

        private async Task ProcessMessageAsync(string message)
        {

            if (message.Contains("\"operation\": \"currentOdometer\""))
            {
                var parsedMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(message);
                float odometer = parsedMessage.odometer;
                OdometerUpdated?.Invoke(this, odometer);
            }

        }

        private async Task SendMessageAsync(string message)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationTokenSource.Token);
        }
    }
}
