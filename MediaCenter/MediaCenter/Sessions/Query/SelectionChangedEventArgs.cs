using System;
using System.Collections.Generic;
using MediaCenter.Media;

namespace MediaCenter.Sessions.Query
{
    public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs args); 
    public class SelectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Notifies that the selected items collection has changed
        /// </summary>
        /// <param name="itemsRemoved">Items that were removed from the selection</param>
        /// <param name="itemsAdded">Items that were added to the selection</param>
        public SelectionChangedEventArgs(List<MediaItem> itemsRemoved, List<MediaItem> itemsAdded)
        {
            ItemsRemoved = itemsRemoved;
            ItemsAdded = itemsAdded;
        }
        /// <summary>
        /// Items that were removed from the selection
        /// </summary>
        public List<MediaItem> ItemsRemoved { get; set; }
        /// <summary>
        /// Items that were added to the selection
        /// </summary>
        public List<MediaItem> ItemsAdded { get; set; }
    }
}
