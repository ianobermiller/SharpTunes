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
using System.Windows.Interop;

namespace SharpTunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        Random random = new Random(Environment.TickCount);
        ViewModel model = new ViewModel();
        Player player = new Player();

        public MainWindow()
        {
            InitializeComponent();
            this.model.Player = this.player;
            this.model.PropertyChanged += ModelPropertyChanged;
            this.DataContext = model;
            this.player.MediaFileFinished += OnMediaFileFinished;
            this.LoadSongs();
            this.Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x319)   // WM_APPCOMMAND message
            {
                // extract cmd from LPARAM (as GET_APPCOMMAND_LPARAM macro does)
                int cmd = (int)((uint)lParam >> 16 & ~0xf000);
                switch (cmd)
                {
                    case 11:  // APPCOMMAND_MEDIA_NEXTTRACK
                        handled = true;
                        break;
                    case 12:  // APPCOMMAND_MEDIA_PREVIOUSTRACK
                        handled = true;
                        break;
                    case 14:  // APPCOMMAND_MEDIA_PLAY_PAUSE
                        this.PlayPause();
                        handled = true;
                        break;
                    default:
                        break;
                }
            }
            return IntPtr.Zero;
        }

        void OnMediaFileFinished()
        {
            // Play something random from the current list
            var list = this.model.SongsView.Cast<MediaFile>().ToArray();
            var currentMedia = this.player.CurrentMediaFile;

            if (list.Length <= 1)
            {
                return;
            }

            while (true)
            {
                var next = list[this.random.Next(0, list.Length)];
                if (next != currentMedia)
                {
                    player.Load(next).Play();
                    return;
                }
            }
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
        }

        private void PlayPause()
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
            this.PlayPause();
        }

        private void uxLibraryRowSelectedHandler(object sender, RoutedEventArgs e)
        {
            var mediaFile = uxLibrary.SelectedItem as MediaFile;
            if (mediaFile == null) return;

            this.player.Load(mediaFile).Play();
        }
    }
}
