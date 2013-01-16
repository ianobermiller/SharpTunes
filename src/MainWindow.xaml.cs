using System.Windows.Input;
using MahApps.Metro.Controls;

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
            model.Player = new Player();
            this.DataContext = model;
            model.Player.Load(@"E:\Dropbox\Music\Taylor Swift - Red\CD 1\08 - We Are Never Ever Getting Back Together.mp3").Play();
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
    }
}
