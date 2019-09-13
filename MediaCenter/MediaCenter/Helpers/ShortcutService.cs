using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;

namespace MediaCenter.Helpers
{
    public class ShortcutService
    {
        private RelayCommand _toggleFavoriteShortcut;
        private RelayCommand _nextShortcut;
        private RelayCommand _previousShortcut;

        public event EventHandler ToggleFavorite;
        public RelayCommand ToggleFavoriteShortcut => _toggleFavoriteShortcut ?? (_toggleFavoriteShortcut = new RelayCommand(RaiseToggleFavorite));
        private void RaiseToggleFavorite()
        {
            ToggleFavorite?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Next;
        public RelayCommand NextShortcut => _nextShortcut ?? (_nextShortcut = new RelayCommand(RaiseNext));
        private void RaiseNext()
        {
            Next?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Previous;
        public RelayCommand PreviousShortcut => _previousShortcut ?? (_previousShortcut = new RelayCommand(RaisePrevious));
        private void RaisePrevious()
        {
            Previous?.Invoke(this, EventArgs.Empty);
        }
    }
}
