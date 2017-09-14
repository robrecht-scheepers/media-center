using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;

namespace VideoPlayerPOC
{
    public class VideoPLayerViewModel : PropertyChangedNotifier
    {
        private string _filePath;

        public VideoPLayerViewModel(string filePath)
        {
            _filePath = filePath;
        }

        public PlayState PlayState
        {
            get { return _playState; }
            set { SetValue(ref _playState, value); }
        }

        private RelayCommand _playCommand;
        private PlayState _playState;

        public RelayCommand PlayCommand
        {
            get { return _playCommand ?? (_playCommand = new RelayCommand(() => PlayState = PlayState.Play)); }
        }
    }
}
