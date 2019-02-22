//using MediaCenter.Media;
//using MediaCenter.MVVM;
//using MediaCenter.Repository;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Configuration;
//using System.Linq;
//using System.Threading.Tasks;

//namespace MediaCenter.Sessions.Query
//{
//    public class QueryResultDetailViewModel : QueryResultViewModel
//    {
//        private readonly int PrefetchBufferSize = int.Parse(ConfigurationManager.AppSettings["PrefetchBufferSize"]);

//        private MediaItem _previousSelectedItem = null;
//        private IRepository _repository;

//        public QueryResultDetailViewModel(ObservableCollection<MediaItem> items, IRepository repository, MediaItem selectedItem = null) : base(repository)
//        {
//            _repository = repository;
//            if (selectedItem != null && Items.Contains(selectedItem))
//                SelectedItem = selectedItem;
//            else if (Items.Any())
//                SelectedItem = Items.First();
//        }

//        private MediaItem _selectedItem;
//        public MediaItem SelectedItem
//        {
//            get => _selectedItem;
//            set => SetValue(ref _selectedItem, value, async () => await SelectedItemChanged(), SelectedItemChanging);
//        }

//        private MediaItemViewModel _selectedItemViewModel;
//        public MediaItemViewModel SelectedItemViewModel
//        {
//            get => _selectedItemViewModel;
//            set => SetValue(ref _selectedItemViewModel, value);
//        }

//        private void SelectedItemChanging()
//        {
//            // store current selected item, so that SelectedItemChanged can do the saving and cleanup
//            _previousSelectedItem = SelectedItem;
//        }

//        private async Task SelectedItemChanged()
//        {
//            SelectedItems.Clear();
//            if(SelectedItem != null)
//                SelectedItems.Add(SelectedItem);

//            //RaiseSelectionChanged(
//            //    _previousSelectedItem == null ? new List<MediaItem>() : new List<MediaItem>{_previousSelectedItem}, 
//            //    SelectedItem == null ? new List<MediaItem>() : new List<MediaItem>{SelectedItem});

//            //SelectedItemViewModel = CreateItemViewModel(SelectedItem);

//            // fetch the new content
//            if (SelectedItem != null && SelectedItem.MediaType == MediaType.Image)
//            {
//                await LoadFullImage(SelectedItem);
//            }
            
//            // TODO: who cleans up the content of the old item?
//        }

//        private async Task LoadFullImage(MediaItem item)
//        {
//            // decide other prefetch items
//            var prefetchList = new List<MediaItem>();
//            var index = Items.IndexOf(item);
//            var prefetchCount = 0;
//            if (index < Items.Count - 1)
//            {
//                for (int i = 1; index + i < Items.Count && prefetchCount < PrefetchBufferSize; i++)
//                {
//                    if (Items[index + i].MediaType == MediaType.Image)
//                    {
//                        prefetchList.Add(Items[index + i]);
//                        prefetchCount++;
//                    }
//                }
//            }
//            if (index > 0 && Items[index - 1].MediaType == MediaType.Image)
//            {
//                prefetchList.Add(Items[index - 1]);
//            }
//            if (item.MediaType == MediaType.Image)
//                item.Content = await _repository.GetFullImage(item, prefetchList);
//        }

//        //private MediaItemViewModel CreateItemViewModel(MediaItem selectedItem)
//        //{
//        //    if (selectedItem == null)
//        //        return null;
//        //    switch (selectedItem.MediaType)
//        //    {
//        //        case MediaType.Image:
//        //            return new ImageItemViewModel(selectedItem);
//        //        case MediaType.Video:
//        //            return new VideoItemViewModel(selectedItem);
//        //        default:
//        //            return null;
//        //    }
//        //}

//        #region Command: Select next item
//        private RelayCommand _selectNextItemCommand;
//        public RelayCommand SelectNextItemCommand
//            => _selectNextItemCommand ?? (_selectNextItemCommand = new RelayCommand(SelectNextItem, CanExecuteSelectNextItem));
//        protected void SelectNextItem()
//        {
//            if (CanExecuteSelectNextItem())
//                SelectedItem = Items[Items.IndexOf(SelectedItem) + 1];
//        }
//        private bool CanExecuteSelectNextItem()
//        {
//            if (SelectedItem == null)
//                return false;
//            return Items.IndexOf(SelectedItem) < Items.Count - 1;
//        }
//        #endregion

//        #region Command: Select previous item
//        private RelayCommand _selectPreviousItemCommand;
//        public RelayCommand SelectPreviousItemCommand
//            => _selectPreviousItemCommand ?? (_selectPreviousItemCommand = new RelayCommand(SelectPreviousItem, CanExecuteSelectPreviousItem));
//        protected void SelectPreviousItem()
//        {
//            SelectedItem = Items[Items.IndexOf(SelectedItem) - 1];
//        }
//        private bool CanExecuteSelectPreviousItem()
//        {
//            if (SelectedItem == null)
//                return false;
//            return Items.IndexOf(SelectedItem) > 0;
//        }
//        #endregion

//    }
//}
