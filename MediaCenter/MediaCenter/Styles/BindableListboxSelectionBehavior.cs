using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using MediaCenter.Media;
using MediaCenter.MVVM;

namespace MediaCenter.Styles
{
    public class BindableListboxSelectionBehavior : Behavior<ListBox>
    {
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
            if(SelectedItems == null) return;

            var tmp = SelectedItems.ToList();

            foreach (var obj in e.RemovedItems)
            {
                var removedMediaItem = (MediaItem)obj;
                if (tmp.Contains(removedMediaItem))
                    tmp.Remove(removedMediaItem);
            }
            foreach (var obj in e.AddedItems)
            {
                var addedMedaiItem = (MediaItem)obj;
                if(!tmp.Contains(addedMedaiItem))
                    tmp.Add(addedMedaiItem);
            }

            SelectedItems.ReplaceAllItems(tmp);
        }

        public BatchObservableCollection<MediaItem> SelectedItems
        {
            get { return (BatchObservableCollection<MediaItem>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(BatchObservableCollection<MediaItem>), typeof(BindableListboxSelectionBehavior), new PropertyMetadata(null));
        
    }
}
