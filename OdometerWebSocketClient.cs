using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TestWPF
{
    public class OdometerWebSocketClient
    {
        private ClientWebSocket websocket;
        private TextBlock odometerValueText;
        private Ellipse statusIndicator;
        private Button startButton;
        private CancellationTokenSource cancellationTokenSource;
        private Task receiveTask;
        private Task sendTask;
        private string url;

        public event EventHandler<string> ConnectError;

        public bool IsRunning { get; private set; }

        public OdometerWebSocketClient(TextBlock odometerValueText, Ellipse statusIndicator,
                                       Button startButton, string url)
        {
            this.odometerValueText = odometerValueText;
            this.statusIndicator = statusIndicator;
            this.startButton = startButton;
            this.url = url;
        }

        public async void Start()
        {
            if (!IsRunning)
            {
                websocket = new ClientWebSocket();
                cancellationTokenSource = new CancellationTokenSource();

                try
                {
                    await websocket.ConnectAsync(new Uri(this.url), cancellationTokenSource.Token);

                    // Запускаем цикл приема
                    receiveTask = ReceiveLoop();

                    // Старт опроса сеервера каждые 10 секунд
                    sendTask = Task.Run(async () =>
                    {
                        while (websocket.State == WebSocketState.Open)
                        {
                            // Запрос на получения данных одометра
                            await SendRequest("getCurrentOdometer");

                            // Запрос на получения статуса одометра
                            float randomOdometerValue = GetRandomOdometerValue();
                            await SendOdometerValue(randomOdometerValue);

                            await Task.Delay(10000);
                        }
                    });


                    startButton.IsEnabled = false;
                    statusIndicator.Fill = Brushes.Green;
                }
                catch (Exception ex)
                {
                    ConnectError?.Invoke(this, ex.Message);
                    statusIndicator.Fill = Brushes.Red;
                    startButton.IsEnabled = true;
                }
            }
        }

        private async Task ReceiveLoop()
        {
            byte[] buffer = new byte[1024];
            while (websocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);


                    ProcessReceivedMessage(message);
                }
            }
        }

        private void ProcessReceivedMessage(string message)
        {

            dynamic parsedMessage = Newtonsoft.Json.JsonConvert.DeserializeObject(message);
            string operation = parsedMessage.operation;

            if (operation == "currentOdometer" || operation == "randomStatus")
            {
                float odometerValue = parsedMessage.odometer;

                // Обновление значений одометра
                odometerValueText.Dispatcher.Invoke(() =>
                {
                    odometerValueText.Text = odometerValue.ToString();
                });
            }
        }

        private async Task SendRequest(string operation)
        {

            var message = new
            {
                operation = operation
            };
            string jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            byte[] buffer = Encoding.UTF8.GetBytes(jsonMessage);

            await websocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationTokenSource.Token);
        }

        private async Task SendOdometerValue(float value)
        {
            var message = new
            {
                operation = "odometer_val",
                value = value
            };
            string jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            byte[] buffer = Encoding.UTF8.GetBytes(jsonMessage);

            await websocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationTokenSource.Token);
        }

        private float GetRandomOdometerValue()
        {
            Random random = new Random();
            return (float)random.NextDouble() * 1000;
        }
    }
}
