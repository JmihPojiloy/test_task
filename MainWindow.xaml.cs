using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;


namespace TestWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConfigLoad config;

        private BackgroundMusicController musicController;
        private SoundEffectController soundController;

        private VLCStreamingClient streamingClient;

        string? url;

        private OdometerWebSocketClient websocketClient;

        public MainWindow()
        {
            InitializeComponent();

            config = new ConfigLoad();
            url = $"ws://{config.ServerAdress}:{config.PortAdress}/ws";
            musicController = new BackgroundMusicController("background_music.mp3");
            soundController = new SoundEffectController("sound_effect.mp3");

            streamingClient = new VLCStreamingClient(videoView);
            streamingClient.StreamingSoundDetected += StreamingClient_StreamingSoundDetected;
            streamingClient.StreamingSilent += StreamingClient_StreamingSilent;

            websocketClient = new OdometerWebSocketClient(OdometerTextBlock, lamp, Start_Button, url);

            websocketClient.ConnectError += WebsocketClient_ErrorNotification;
        }

        private async void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            Menu_Port_Address.Text = config.PortAdress.ToString();
            Menu_Server_Address.Text = config.ServerAdress.ToString();
            Menu_Video_Address.Text = streamingClient.StreamUrl;

            musicController.Play();

            if (!websocketClient.IsRunning)
            {
                websocketClient.Start();
            }
        }

        private void WebsocketClient_ErrorNotification(object sender, string errorMessage)
        {
            MessageBox.Show("WebSocket Error: " + errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Sound_Click(object sender, RoutedEventArgs e)
        {
            soundController.ToggleSound();
        }

        private void Music_Click(object sender, RoutedEventArgs e)
        {
            if (musicController.IsPlaying)
                musicController.Pause();
            else
                musicController.Play();
        }

        private void Menu_Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double volume = Menu_Volume.Value / Menu_Volume.Maximum;
            musicController.SetVolume(volume);
            soundController.SetVolume(volume);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            streamingClient.StartStreaming();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            streamingClient.StopStreaming();
        }

        private void StreamingClient_StreamingSoundDetected(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => musicController.Pause());
        }

        private void StreamingClient_StreamingSilent(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => musicController.Play());
        }

    }
}
