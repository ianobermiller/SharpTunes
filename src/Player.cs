﻿using System;
using System.ComponentModel;
using System.IO;
using System.Timers;
using NAudio.Wave;

namespace SharpTunes
{
    class Player : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public TimeSpan CurrentTime { get; set; }
        public TimeSpan TotalTime { get; set; }
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

            this.mp3 = new Mp3FileReader(fileName);
            this.TotalTime = this.mp3.TotalTime;

            var volumeStream = new WaveChannel32(mp3);
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
