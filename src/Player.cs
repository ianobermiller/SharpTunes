using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using NAudio.Wave;
using Newtonsoft.Json;

namespace SharpTunes
{
    class Player : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MediaFile CurrentMediaFile { get; set; }
        public string CurrentAlbumArt { get; set; }
        public TimeSpan CurrentTime { get; set; }
        public TimeSpan TotalTime { get; set; }
        public bool IsPlaying { get; set; }

        public double SeekMilliseconds { 
            get
            {
                return this.CurrentTime.TotalMilliseconds;
            }
            set
            {
                mp3.CurrentTime = TimeSpan.FromMilliseconds(value);
            }
        }

        private WaveStream mp3 = null;
        private WaveOutEvent player = new WaveOutEvent();
        private Timer timer = new Timer(100);

        public Player()
        {
            this.timer.Elapsed += OnTimerElapsed;
        }

        void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (this.mp3 != null)
            {
                this.CurrentTime = this.mp3.CurrentTime;
            }
        }

        public Player Load(MediaFile song)
        {
            if (this.mp3 != null)
            {
                this.mp3.Close();
            }

            this.CurrentMediaFile = song;

            this.FindAlbumArt();

            this.mp3 = new Mp3FileReader(song.Path);
            this.TotalTime = this.mp3.TotalTime;

            var volumeStream = new WaveChannel32(mp3);
            this.player.Init(volumeStream);
            return this;
        }

        public async void FindAlbumArt()
        {
            this.CurrentAlbumArt = null;
            this.CurrentAlbumArt = await Library.GetAlbumArt(this.CurrentMediaFile);
        }

        public Player Play()
        {
            this.player.Play();
            this.IsPlaying = true;
            this.timer.Start();
            return this;
        }

        public Player Pause()
        {
            this.IsPlaying = false;
            this.player.Pause();
            return this;
        }

        public Player Stop()
        {
            this.Pause();
            this.mp3.Seek(0, SeekOrigin.Begin);
            return this;
        }

        public void Dispose()
        {
            if (this.mp3 != null)
            {
                this.mp3.Dispose();
            }

            this.timer.Dispose();
            this.player.Dispose();
        }
    }
}
