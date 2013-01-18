using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SharpTunes
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<MediaFile> Songs { get; set; }

        public Player Player { get; set; }
    }
}
