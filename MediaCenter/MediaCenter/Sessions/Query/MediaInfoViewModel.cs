using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query
{
    public class MediaInfoViewModel : PropertyChangedNotifier
    {
        private bool _favorite;
        private DateTime _dateTaken;

        public MediaInfoViewModel(MediaInfo info)
        {
            Name = info.Name;
            Tags = new ObservableCollection<string>(info.Tags);
            Favorite = info.Favorite;
            _dateTaken = info.DateTaken;
        }

        public ObservableCollection<string> Tags { get; private set; }

        public bool Favorite
        {
            get { return _favorite; }
            set { SetValue(ref _favorite, value); }
        }

        public string Name { get; }

        public MediaInfo MediaInfo => new MediaInfo(Name)
        {
            Tags = Tags.ToList(),
            Favorite = Favorite,
            DateTaken = _dateTaken
        };
    }
}
