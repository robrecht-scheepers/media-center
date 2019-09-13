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
        
        public event EventHandler ToggleFavorite;
        public RelayCommand ToggleFavoriteShortcut => _toggleFavoriteShortcut ?? (_toggleFavoriteShortcut = new RelayCommand(RaiseToggleFavorite));
        private void RaiseToggleFavorite()
        {
            ToggleFavorite?.Invoke(this, EventArgs.Empty);
        }


    }
}
