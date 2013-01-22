using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.WindowsAPICodePack.Shell;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel;

namespace SharpTunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        ViewModel model = new ViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.model.Player = new Player();
            this.model.PropertyChanged += ModelPropertyChanged;
            this.DataContext = model;
            this.LoadSongs();
        }

        void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Query")
            {
                if (!string.IsNullOrWhiteSpace(this.model.Query))
                {
                    this.model.SongsView.Filter = o =>
                    {
                        var media = (o as MediaFile);
                        return media.Title.ToLowerInvariant().Contains(this.model.Query.ToLowerInvariant());
                    };
                }
                else
                {
                    this.model.SongsView.Filter = null;
                }
            }
        }

        private void LoadSongs()
        {
            this.model.Songs = Library.GetMedia();
            this.model.SongsView = new ListCollectionView(this.model.Songs);
            this.model.Player.Load(this.model.Songs.First()).Play();
        }

        //
        // Handle clicking on the progress bar to change the current time
        //

        private void SetProgressBarValue(double mousePosition)
        {
            var totalMs = model.Player.TotalTime.TotalMilliseconds;
            var ratio = mousePosition / this.uxProgress.ActualWidth;
            var newValue = ratio * totalMs;
            model.Player.SeekMilliseconds = newValue;
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

        private void uxPlayPauseClickHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            if (model.Player.IsPlaying)
            {
                model.Player.Pause();
            }
            else
            {
                model.Player.Play();
            }
        }
    }
}
