using System;
using MediaCenter.Media;
using MediaCenter.MVVM;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using MediaCenter.Helpers;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Query
{
    public class QueryResultViewModel : PropertyChangedNotifier
    {
        private readonly IRepository _repository;
        private RelayCommand _selectNextItemCommand;
        private RelayCommand _selectPreviousItemCommand;

        public QueryResultViewModel(IRepository repository, ShortcutService shortcutService)
        {
            _repository = repository;
            var shortcutService1 = shortcutService;
            Items = new ObservableCollection<MediaItem>();
            SelectedItems = new BatchObservableCollection<MediaItem>();
            SelectedItems.CollectionChanged += SelectedItemsOnCollectionChanged;

            shortcutService1.Next += (s, a) =>
            {
                if (CanExecuteSelectNextItem())
                    SelectNextItem();
            };
            shortcutService1.Previous += (s, a) =>
            {
                if (CanExecuteSelectPreviousItem())
                    SelectPreviousItem();
            };
        }

        public ObservableCollection<MediaItem> Items { get; }

        public BatchObservableCollection<MediaItem> SelectedItems { get; }

        public MediaItem SelectedItem
        {
            get => SelectedItems.FirstOrDefault();
            set => SelectedItems.ReplaceAllItems(new List<MediaItem>{value});
        } 

        public event EventHandler SelectionChanged;

        protected void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            RaisePropertyChanged("SelectedItem");
        }

        public async Task LoadQueryResult(List<MediaItem> queryResult)
        {
            SelectedItems.ReplaceAllItems(new List<MediaItem>());
            Items.Clear();

            queryResult.Sort((x, y) => DateTime.Compare(x.DateTaken, y.DateTaken));
            foreach (var mediaItem in queryResult)
            {
                Items.Add(mediaItem);
            }

            if(Items.Any())
                SelectedItems.Add(Items.First());

            foreach (var mediaItem in Items)
            {
                mediaItem.Thumbnail = await _repository.GetThumbnail(mediaItem);
            }
        }

        public void RemoveItem(MediaItem item)
        {
            SelectedItems.Remove(item);
            Items.Remove(item);
        }

        #region Command: Select next item
        public RelayCommand SelectNextItemCommand
            => _selectNextItemCommand ?? (_selectNextItemCommand = new RelayCommand(SelectNextItem, CanExecuteSelectNextItem));
        public void SelectNextItem()
        {
            if(!Items.Any())
                return;

            var nextIndex = SelectedItems.Any()
                ? Items.IndexOf(SelectedItems.First()) + 1
                : 0;

            if(nextIndex >= Items.Count)
                return;
            
            SelectedItems.ReplaceAllItems(new List<MediaItem>{Items[nextIndex]});
        }
        public bool CanExecuteSelectNextItem()
        {
            return Items.Any() 
                   && (!SelectedItems.Any() || Items.IndexOf(SelectedItems.First()) + 1 < Items.Count);
        }
        #endregion

        #region Command: Select previous item
        public RelayCommand SelectPreviousItemCommand
            => _selectPreviousItemCommand ?? (_selectPreviousItemCommand = new RelayCommand(SelectPreviousItem, CanExecuteSelectPreviousItem));
        public void SelectPreviousItem()
        {
            if (!Items.Any() || !SelectedItems.Any())
                return;

            var nextIndex = Items.IndexOf(SelectedItems.First()) - 1;
                
            if (nextIndex < 0)
                return;

            SelectedItems.ReplaceAllItems(new List<MediaItem> { Items[nextIndex] });
        }
        public bool CanExecuteSelectPreviousItem()
        {
            return Items.Any() && SelectedItems.Any() && Items.IndexOf(SelectedItems.First()) > 0;
        }
        #endregion

    }
}
