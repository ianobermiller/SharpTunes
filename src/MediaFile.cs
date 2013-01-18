using System.ComponentModel;

namespace SharpTunes
{
    public class MediaFile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Path { get; set; }
        public bool HasError { get; set; }
    }
}
