using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using MediaCenter.Media;

namespace MediaCenter.Styles
{
    public class BindableListboxSelectionBehavior : Behavior<ListBox>
    {
        private ObservableCollection<MediaItem> _selection;
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            base.OnDetaching();
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var eRemovedItem in e.RemovedItems)
            {
                var removedMediaItem = (MediaItem) eRemovedItem;
                if (_selection.Contains(removedMediaItem))
                    _selection.Remove(removedMediaItem);
            }
            foreach (var eAddedItem in e.AddedItems)
            {
                var addedMedaiItem = (MediaItem) eAddedItem;
                if(!_selection.Contains(addedMedaiItem))
                    _selection.Add(addedMedaiItem);
            }
        }

        public ObservableCollection<MediaItem> SelectedItems
        {
            get { return (ObservableCollection<MediaItem>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(ObservableCollection<MediaItem>), typeof(BindableListboxSelectionBehavior), new PropertyMetadata(null,SelectedItemsChanged));

        private static void SelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (BindableListboxSelectionBehavior) d;
            me._selection = e.NewValue as ObservableCollection<MediaItem>;
        }
    }
}
