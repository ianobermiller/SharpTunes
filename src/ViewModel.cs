using System.ComponentModel;

namespace SharpTunes
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Player Player { get; set; }
    }
}
