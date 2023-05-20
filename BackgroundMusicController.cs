using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TestWPF
{
    /// <summary>
    /// класс работы с фоновой музыкой
    /// </summary>
    public class BackgroundMusicController
    {
        private MediaPlayer mediaPlayer;
        public bool IsPlaying { get; private set; }

        public BackgroundMusicController(string musicFilePath)
        {
            mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(musicFilePath, UriKind.RelativeOrAbsolute));
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            IsPlaying = false;
        }

        public void Play()
        {
            mediaPlayer.Play();
            IsPlaying = true;
        }

        public void Pause()
        {
            mediaPlayer.Pause();
            IsPlaying = false;
        }

        public void SetVolume(double volume)
        {
            mediaPlayer.Volume = volume;
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            mediaPlayer.Position = TimeSpan.Zero;
            mediaPlayer.Play();
        }
    }
}
