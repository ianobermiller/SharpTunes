using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.WindowsAPICodePack.Shell;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel;
using System;
using System.Windows;

namespace SharpTunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        ViewModel model = new ViewModel();
        Player player = new Player();

        public MainWindow()
        {
            InitializeComponent();
            this.model.Player = this.player;
            this.model.PropertyChanged += ModelPropertyChanged;
            this.DataContext = model;
            this.LoadSongs();
        }

        void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Query")
            {
                this.Search(this.model.Query);
            }
        }

        private void Search(string query)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                this.model.SongsView.Filter = o =>
                {
                    var media = (o as MediaFile);
                    var terms = query.ToLowerInvariant().Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var targets = new[] { media.Title, media.Artist, media.Album }.Select(t => t.ToLowerInvariant());
                    return terms.All(term => targets.Any(target => target.Contains(term)));
                };
            }
            else
            {
                this.model.SongsView.Filter = null;
            }
        }

        private void LoadSongs()
        {
            this.model.Songs = Library.GetMedia();
            this.model.SongsView = new ListCollectionView(this.model.Songs);
            this.player.Load(this.model.Songs.First()).Play();
        }

        //
        // Handle clicking on the progress bar to change the current time
        //

        private void SetProgressBarValue(double mousePosition)
        {
            var totalMs = this.player.TotalTime.TotalMilliseconds;
            var ratio = mousePosition / this.uxProgress.ActualWidth;
            var newValue = ratio * totalMs;
            this.player.SeekMilliseconds = newValue;
        }

        private void uxProgressMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double mousePosition = e.GetPosition(this.uxProgress).X;
            SetProgressBarValue(mousePosition);
        }

        private void uxProgressMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double mousePosition = e.GetPosition(this.uxProgress).X;
                SetProgressBarValue(mousePosition);
            }
        }

        private void uxPlayPauseClickHandler(object sender, RoutedEventArgs e)
        {
            if (this.player.IsPlaying)
            {
                this.player.Pause();
            }
            else
            {
                this.player.Play();
            }
        }

        private void uxLibraryRowSelectedHandler(object sender, RoutedEventArgs e)
        {
            var mediaFile = uxLibrary.SelectedItem as MediaFile;
            if (mediaFile == null) return;

            this.player.Load(mediaFile).Play();
        }
    }
}
