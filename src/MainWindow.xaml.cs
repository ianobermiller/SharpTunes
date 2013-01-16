using MahApps.Metro.Controls;

namespace SharpTunes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        Player player = new Player();

        public MainWindow()
        {
            InitializeComponent();
            player.Load(@"E:\Dropbox\Music\Taylor Swift - Red\CD 1\08 - We Are Never Ever Getting Back Together.mp3").Play();
        }
    }
}
