using System;
using System.Collections.Generic;
using MediaCenter.Media;

namespace MediaCenter.Sessions.Query
{
    public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs args); 
    public class SelectionChangedEventArgs : EventArgs
    {
        public SelectionChangedEventArgs(List<MediaItem> oldSelection, List<MediaItem> newSelection)
        {
            OldSelection = oldSelection;
            NewSelection = newSelection;
        }
        public List<MediaItem> OldSelection { get; set; }
        public List<MediaItem> NewSelection { get; set; }
    }
}
