using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using Id3;
using NAudio.Wave;
using Newtonsoft.Json;

namespace SharpTunes
{
    class Player : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string CurrentTitle { get; set; }
        public string CurrentAlbum { get; set; }
        public string CurrentArtist { get; set; }
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
            this.timer.Start();
        }

        void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (this.mp3 != null)
            {
                this.CurrentTime = this.mp3.CurrentTime;
            }
        }

        public Player Load(string fileName)
        {
            if (this.mp3 != null)
            {
                this.mp3.Close();
            }

            using (var tagReader = new Mp3File(fileName))
            {
                var tag = tagReader.GetTag(Id3TagFamily.FileStartTag);
                this.CurrentTitle = tag.Title.Value;
                this.CurrentArtist = tag.Artists.Values.First();
                this.CurrentAlbum = tag.Album.Value;
            }

            this.FindAlbumArt();

            this.mp3 = new Mp3FileReader(fileName);
            this.TotalTime = this.mp3.TotalTime;

            var volumeStream = new WaveChannel32(mp3);
            this.player.Init(volumeStream);
            return this;
        }

        public async Task FindAlbumArt()
        {
            var client = new HttpClient();
            var url = "http://ws.audioscrobbler.com/2.0/?method=album.search&album=" +
                HttpUtility.UrlEncode(this.CurrentArtist) + "+" + 
                HttpUtility.UrlEncode(this.CurrentAlbum) + 
                "&api_key=009a482cfc59173fb361faa0b5c49b06&format=json";
            var json = await client.GetStringAsync(url);
            var jsonSerializer = new JsonSerializer();
            dynamic result = jsonSerializer.Deserialize(new JsonTextReader(new StringReader(json)));
            this.CurrentAlbumArt = result.results.albummatches.album[0].image[1]["#text"];
        }

        public Player Play()
        {
            this.player.Play();
            this.IsPlaying = true;
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
