using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Windows;

/// <summary>
/// класс работы с видеосервисом
/// </summary>
public class VLCStreamingClient
{
    private LibVLC libVLC;
    private MediaPlayer mediaPlayer;
    private VideoView videoView;
    private IWaveIn waveIn;
    private string streamUrl = "rtsp://";

    public event EventHandler StreamingSoundDetected;
    public event EventHandler StreamingSilent;

    public string StreamUrl => streamUrl;

    public VLCStreamingClient(VideoView view)
    {
        Core.Initialize();
        libVLC = new LibVLC();
        mediaPlayer = new MediaPlayer(libVLC);
        videoView = view;
        videoView.MediaPlayer = mediaPlayer;
    }

    public void StartStreaming()
    {
        // Start streaming
        Media media = new Media(libVLC, streamUrl, FromType.FromLocation);
        mediaPlayer.Play(media);

        // Start monitoring audio
        waveIn = new WasapiLoopbackCapture();
        waveIn.DataAvailable += WaveIn_DataAvailable;
        waveIn.StartRecording();

    }

    public void StopStreaming()
    {
        // Stop streaming
        mediaPlayer.Stop();

        // Stop monitoring audio
        waveIn.StopRecording();
        waveIn.Dispose();

    }

    private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
    {
        // Check if audio data is present
        for (int i = 0; i < e.BytesRecorded; i += 2)
        {
            short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
            if (sample != 0)
            {
                // Streaming broadcast has sound, raise event
                StreamingSoundDetected?.Invoke(this, EventArgs.Empty);
                return;
            }
        }

        // Streaming broadcast is silent, raise event
        StreamingSilent?.Invoke(this, EventArgs.Empty);
    }

}
