using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace SharpTunes
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<MediaFile> Songs { get; set; }
        public ICollectionView SongsView { get; set; }
        public string Query { get; set; }
        public Player Player { get; set; }
    }
}
