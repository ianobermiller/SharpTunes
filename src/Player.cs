using System.IO;
using NAudio.Wave;

namespace SharpTunes
{
    class Player
    {
        private WaveStream mainOutputStream = null;
        private WaveOutEvent player = new WaveOutEvent();

        public Player Load(string fileName)
        {
            if (this.mainOutputStream != null)
            {
                this.mainOutputStream.Close();
            }

            this.mainOutputStream = new Mp3FileReader(fileName);
            WaveChannel32 volumeStream = new WaveChannel32(mainOutputStream);
            this.player.Init(volumeStream);
            return this;
        }

        public Player Play()
        {
            this.player.Play();
            return this;
        }

        public Player Pause()
        {
            this.player.Pause();
            return this;
        }

        public Player Stop()
        {
            player.Pause();
            this.mainOutputStream.Seek(0, SeekOrigin.Begin);
            return this;
        }
    }
}
