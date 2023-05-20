using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TestWPF
{
    /// <summary>
    /// класс работы со звуковыми эффектами
    /// </summary>
    public class SoundEffectController
    {
        private MediaPlayer mediaPlayer;
        public bool IsEnabled { get; private set; }

        public SoundEffectController(string soundFilePath)
        {
            mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(soundFilePath, UriKind.RelativeOrAbsolute));
            IsEnabled = true;
        }

        public void ToggleSound()
        {
            IsEnabled = !IsEnabled;
        }

        public void SetVolume(double volume)
        {
            mediaPlayer.Volume = volume;
        }

        public void Play()
        {
            if (IsEnabled)
                mediaPlayer.Play();
        }
    }
}
