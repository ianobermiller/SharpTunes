using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Microsoft.WindowsAPICodePack.Shell;
using System.Linq;
using System.Collections.ObjectModel;

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
            this.model.Songs = new ObservableCollection<MediaFile>();
            this.model.Player = new Player();
            this.DataContext = model;
            this.LoadSongs();
        }

        private void LoadSongs()
        {
            using (ShellLibrary shellLibrary = ShellLibrary.Load("Music", true))
            {
                var mp3s = shellLibrary.SelectMany((ShellFileSystemFolder f) => Directory.EnumerateFiles(f.Path, "*.mp3", SearchOption.AllDirectories));

                foreach (var path in mp3s)
                {
                    try
                    {
                        using (var file = TagLib.File.Create(path))
                        {
                            var song = new MediaFile()
                            {
                                Title = file.Tag.Title,
                                Artist = file.Tag.FirstAlbumArtist ?? file.Tag.FirstPerformer,
                                Album = file.Tag.Album,
                                Path = path
                            };
                            this.model.Songs.Add(song);
                        }
                    }
                    catch
                    {
                        var song = new MediaFile()
                        {
                            Path = path,
                            HasError = true
                        };
                        this.model.Songs.Add(song);
                    }
                }
            }
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
