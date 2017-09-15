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
        
        public VideoPLayerViewModel(string filePath)
        {
            _filePath = filePath;
            PlayState = PlayState.Stopped;
        }

        public PlayState PlayState
        {
            get { return _playState; }
            set { SetValue(ref _playState, value); }
        }

        private string _filePath;
        private RelayCommand _playCommand;
        private RelayCommand _loadCommand;
        private RelayCommand _pauseCommand;
        private RelayCommand _stopCommand;
        private PlayState _playState;
        private string _videoFilePath;


        public RelayCommand LoadCommand
        {
            get { return _loadCommand ?? (_loadCommand = new RelayCommand(() => VideoFilePath = _filePath)); }
        }

        public RelayCommand PlayCommand
        {
            get { return _playCommand ?? (_playCommand = new RelayCommand(() => PlayState = PlayState.Play)); }
        }
        public RelayCommand StopCommand
        {
            get { return _stopCommand ?? (_stopCommand = new RelayCommand(() => PlayState = PlayState.Stopped)); }
        }
        public RelayCommand PauseCommand
        {
            get { return _pauseCommand ?? (_pauseCommand = new RelayCommand(() => PlayState = PlayState.Paused)); }
        }
        
        public string VideoFilePath
        {
            get { return _videoFilePath; }
            set { SetValue(ref _videoFilePath, value); }
        }
    }
}
