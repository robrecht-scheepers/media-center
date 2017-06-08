using System.Collections.ObjectModel;
using System.Linq;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query
{
    public class MediaInfoViewModel : PropertyChangedNotifier
    {
        private readonly MediaInfo _info;

        public MediaInfoViewModel(MediaInfo info)
        {
            _info = info.Clone();
            Tags = new ObservableCollection<string>(_info.Tags);
            Favorite = _info.Favorite;
        }

        public ObservableCollection<string> Tags { get; private set; }

        private bool _favorite;
        public bool Favorite
        {
            get { return _favorite; }
            set { SetValue(ref _favorite, value); }
        }

        public MediaInfo MediaInfo
        {
            get
            {
                _info.Tags = Tags.ToList();
                _info.Favorite = Favorite;
                return _info.Clone();
            }
        }
        
    }
}
