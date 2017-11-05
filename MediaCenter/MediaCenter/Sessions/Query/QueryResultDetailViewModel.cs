using MediaCenter.Media;
using MediaCenter.MVVM;
using MediaCenter.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MediaCenter.Sessions.Query
{
    public class QueryResultDetailViewModel : QueryResultViewModel
    {
        private readonly int PrefetchBufferSize = int.Parse(ConfigurationManager.AppSettings["PrefetchBufferSize"]);

        private MediaItem _previousSelectedItem = null;
        private IRepository _repository;

        public QueryResultDetailViewModel(ObservableCollection<MediaItem> queryResultItems, IRepository repository) : base(queryResultItems)
        {
            _repository = repository;
            if (QueryResultItems.Any())
                SelectedItem = QueryResultItems.First();
        }

        private MediaItem _selectedItem;
        public MediaItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetValue(ref _selectedItem, value, async () => await SelectedItemChanged(), SelectedItemChanging);
            }
        }

        private MediaItemViewModel _selectedItemViewModel;
        public MediaItemViewModel SelectedItemViewModel
        {
            get { return _selectedItemViewModel; }
            set { SetValue(ref _selectedItemViewModel, value); }
        }

        private void SelectedItemChanging()
        {
            // store current selected item, so that SelectedItemChanged can do the saving and cleanup
            _previousSelectedItem = SelectedItem;
        }

        private async Task SelectedItemChanged()
        {
            SelectedItems.Clear();
            if(SelectedItem != null)
                SelectedItems.Add(SelectedItem);

            RaiseSelectionChanged(
                _previousSelectedItem == null ? new List<MediaItem>() : new List<MediaItem>{_previousSelectedItem}, 
                SelectedItem == null ? new List<MediaItem>() : new List<MediaItem>{SelectedItem});

            SelectedItemViewModel = CreateItemViewModel(SelectedItem);

            // fetch the new content
            if (SelectedItem != null && SelectedItem.MediaType == MediaType.Image)
            {
                await LoadFullImage(SelectedItem);
            }
            
            // TODO: who cleans up the content of the old item?
        }

        private async Task LoadFullImage(MediaItem item)
        {
            // decide other prefetch items
            var prefetchList = new List<string>();
            var index = QueryResultItems.IndexOf(item);
            var prefetchCount = 0;
            if (index < QueryResultItems.Count - 1)
            {
                for (int i = 1; index + i < QueryResultItems.Count && prefetchCount < PrefetchBufferSize; i++)
                {
                    if (QueryResultItems[index + i].MediaType == MediaType.Image)
                    {
                        prefetchList.Add(QueryResultItems[index + i].Name);
                        prefetchCount++;
                    }
                }
            }
            if (index > 0 && QueryResultItems[index - 1].MediaType == MediaType.Image)
            {
                prefetchList.Add(QueryResultItems[index - 1].Name);
            }
            if (item.MediaType == MediaType.Image)
                item.Content = await _repository.GetFullImage(item.Name, prefetchList);
        }

        private MediaItemViewModel CreateItemViewModel(MediaItem selectedItem)
        {
            if (selectedItem == null)
                return null;
            switch (selectedItem.MediaType)
            {
                case MediaType.Image:
                    return new ImageItemViewModel(selectedItem);
                case MediaType.Video:
                    return new VideoItemViewModel(selectedItem);
                default:
                    return null;
            }
        }

        #region Command: Select next image
        private RelayCommand _selectNextImageCommand;
        public RelayCommand SelectNextImageCommand
            => _selectNextImageCommand ?? (_selectNextImageCommand = new RelayCommand(SelectNextImage, CanExecuteSelectNextImage));
        private void SelectNextImage()
        {
            if (CanExecuteSelectNextImage())
                SelectedItem = QueryResultItems[QueryResultItems.IndexOf(SelectedItem) + 1];
        }
        private bool CanExecuteSelectNextImage()
        {
            if (SelectedItem == null)
                return false;
            return QueryResultItems.IndexOf(SelectedItem) < QueryResultItems.Count - 1;
        }
        #endregion

        #region Command: Select previous image
        private RelayCommand _selectPreviousImageCommand;
        public RelayCommand SelectPreviousImageCommand
            => _selectPreviousImageCommand ?? (_selectPreviousImageCommand = new RelayCommand(SelectPreviousImage, CanExecuteSelectPreviousImage));
        private void SelectPreviousImage()
        {
            SelectedItem = QueryResultItems[QueryResultItems.IndexOf(SelectedItem) - 1];
        }
        private bool CanExecuteSelectPreviousImage()
        {
            if (SelectedItem == null)
                return false;
            return QueryResultItems.IndexOf(SelectedItem) > 0;
        }
        #endregion

    }
}
