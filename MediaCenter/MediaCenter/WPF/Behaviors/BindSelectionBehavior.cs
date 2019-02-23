using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using MediaCenter.Media;
using MediaCenter.MVVM;

namespace MediaCenter.WPF.Behaviors
{
    public class BindSelectionBehavior : Behavior<ListBox>
    {
        private bool _applyingSelectionFromSource;
        private bool _applyingSelectionFromList;

        private BatchObservableCollection<MediaItem> _previousSelectedItems; // needed to unregister the event after replacement

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

        public BatchObservableCollection<MediaItem> SelectedItems
        {
            get { return (BatchObservableCollection<MediaItem>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(BatchObservableCollection<MediaItem>), typeof(BindSelectionBehavior), new PropertyMetadata(null, SelectedItemsSourceChanged));

        private static void SelectedItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is BindSelectionBehavior me))
                return;

            if (me._previousSelectedItems != null)
            {
                me._previousSelectedItems.CollectionChanged -= me.SelectedItemsOnCollectionChanged;
            }
            me._previousSelectedItems = me.SelectedItems;

            me._applyingSelectionFromSource = true;
            me.AssociatedObject.SelectedItems.Clear();
            if (me.SelectedItems != null)
            {
                foreach (var item in me.SelectedItems)
                {
                    me.AssociatedObject.SelectedItems.Add(item);
                }
                me.SelectedItems.CollectionChanged += me.SelectedItemsOnCollectionChanged;
            }

            me._applyingSelectionFromSource = false;
            
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_applyingSelectionFromList)
                return;

            _applyingSelectionFromSource = true;
            AssociatedObject.SelectedItems.Clear();
            foreach (var item in SelectedItems)
            {
                AssociatedObject.SelectedItems.Add(item);
            }
            _applyingSelectionFromSource = false;
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_applyingSelectionFromSource)
                return;

            if (SelectedItems == null)
                return;

            if(AssociatedObject.Items.Count == 0) // Items were cleared because switched to detail view --> selection should not be cleared
                return;

            _applyingSelectionFromList = true;
            SelectedItems.ReplaceAllItems(AssociatedObject.SelectedItems.OfType<MediaItem>());
            _applyingSelectionFromList = false;
        }
    }
}
