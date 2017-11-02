using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MediaCenter.Media;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Query.Filters;
using MediaCenter.Sessions.Slideshow;
using MediaCenter.Sessions.Tags;
using Microsoft.Win32;
using System.Windows;

namespace MediaCenter.Sessions.Query
{
    public class QuerySessionViewModel : SessionViewModelBase
    {
        private MediaItem _previousSelectedItem = null;
        
        public QuerySessionViewModel(SessionBase session) : base(session)
        {
            InitializeFilterCollectionViewModel();
        }

        public override string Name => "View media";

        public QuerySession QuerySession => (QuerySession) Session;

        public ObservableCollection<MediaItem> QueryResult => QuerySession.QueryResult; 

        #region Result items
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
            // stor current selected item, so that SelectedItemChanged can do the saving and cleanup
            _previousSelectedItem = SelectedItem;
        }

        private async Task SelectedItemChanged()
        {
            SelectedItemViewModel = CreateItemViewModel(SelectedItem);

            // start fetching the new content
            Task contentTask = null;
            if (SelectedItem != null)
            {
                Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt ss.fff")} | SelectedItemChanged Requesting{SelectedItem.Name}");
                contentTask = QuerySession.LoadFullImage(SelectedItem.Name);
            }

            Task saveTask = null;
            if (_previousSelectedItem != null)
            {
                if (TagsViewModel.IsDirty)
                {
                    _previousSelectedItem.Tags = TagsViewModel.SelectedTags;
                    _previousSelectedItem.IsInfoDirty = true;
                }
                saveTask = QuerySession.SaveItem(_previousSelectedItem);
            }

            // Setup tags
            InitializeTagsViewModel();

            // wait for the new content
            if (contentTask != null)
            {
                Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt ss.fff")} | SelectedItemChanged Awaiting {SelectedItem.Name}");
                await contentTask;
                Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt ss.fff")} | SelectedItemChanged Received {SelectedItem.Name}");
            }

            // wait for the saving task and then clean up the content of the previous item
            {
                if (saveTask != null)
                {
                    await saveTask;
                    _previousSelectedItem.Content = null;
                }
            }
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
            if(CanExecuteSelectNextImage())
                SelectedItem = QueryResult[QueryResult.IndexOf(SelectedItem) + 1];
        }
        private bool CanExecuteSelectNextImage()
        {
            if (SelectedItem == null)
                return false;
            return QueryResult.IndexOf(SelectedItem) < QueryResult.Count - 1;
        }
        #endregion

        #region Command: Select previous image
        private RelayCommand _selectPreviousImageCommand;
        public RelayCommand SelectPreviousImageCommand
            => _selectPreviousImageCommand ?? (_selectPreviousImageCommand = new RelayCommand(SelectPreviousImage, CanExecuteSelectPreviousImage));
        private void SelectPreviousImage()
        {
            SelectedItem = QueryResult[QueryResult.IndexOf(SelectedItem) - 1];
        }
        private bool CanExecuteSelectPreviousImage()
        {
            if (SelectedItem == null)
                return false;
            return QueryResult.IndexOf(SelectedItem) > 0;
        }
        #endregion

        #region Command: delete current image
        private AsyncRelayCommand _deleteCurrentImageCommand;
        public AsyncRelayCommand DeleteCurrentImageCommand
            => _deleteCurrentImageCommand ?? (_deleteCurrentImageCommand = new AsyncRelayCommand(DeleteCurrentImage, CanExecuteDeleteCurrentImage));
        private async Task DeleteCurrentImage()
        {
            var confirmationResult = MessageBox.Show("Are you sure you want to delete this item from the repository? This action cannot be undone.", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Hand);
            if (confirmationResult == MessageBoxResult.Yes)
            {
                var index = QueryResult.IndexOf(SelectedItem);
                await QuerySession.DeleteItem(SelectedItem);
                if (QueryResult.Count > index)
                    SelectedItem = QueryResult[index]; // show next item, now at the same index of deleted item
                else if (QueryResult.Any())
                    SelectedItem = QueryResult.Last(); // we deleted the last item, show the current last item
                else
                    SelectedItem = null; // the list is empty now as we deleted the only element, show nothing
            }
        }
        private bool CanExecuteDeleteCurrentImage()
        {
            return (SelectedItem != null);
        }
        #endregion

        #region Command: save current item to file
        private AsyncRelayCommand _saveCurrentItemToFileCommand;
        public AsyncRelayCommand SaveCurrentItemToFileCommand
            => _saveCurrentItemToFileCommand ?? (_saveCurrentItemToFileCommand = new AsyncRelayCommand(SaveCurrentItemToFile, CanExecuteSaveCurrentItemToFile));
        private async Task SaveCurrentItemToFile()
        {
            if (SelectedItem == null)
                return;

            var dialog = new SaveFileDialog() {
                FileName = SelectedItem.ContentFileName
            };
            dialog.ShowDialog();
            if(!string.IsNullOrEmpty(dialog.FileName))
            {
                await QuerySession.Repository.SaveContentToFile(SelectedItem.Name, dialog.FileName);
            }                        
        }
        private bool CanExecuteSaveCurrentItemToFile()
        {
            return (SelectedItem != null);
        }
        #endregion

        #endregion

        #region Tags
        private TagsViewModel _tagsViewModel;
        public TagsViewModel TagsViewModel
        {
            get { return _tagsViewModel; }
            set { SetValue(ref _tagsViewModel, value); }
        }

        private void InitializeTagsViewModel()
        {
            TagsViewModel = new TagsViewModel(QuerySession.Repository.Tags, SelectedItem?.Tags);            
        }

        private RelayCommand _copyTagsFromPreviousCommand;
        public RelayCommand CopyTagsFromPreviousCommand => _copyTagsFromPreviousCommand ?? (_copyTagsFromPreviousCommand = new RelayCommand(CopyTagsFromPrevious));

        public void CopyTagsFromPrevious()
        {
            if(SelectedItem == null)
                return;
            var index = QueryResult.IndexOf(SelectedItem);
            if (index <= 0)
                return;
            foreach (var tag in QueryResult[index - 1].Tags)
            {
                if(!SelectedItem.Tags.Contains(tag))
                    SelectedItem.Tags.Add(tag);
            }
            InitializeTagsViewModel();
        }

        public bool CanExecuteCopyTagFromPrevious()
        {
            if (SelectedItem == null)
                return false;
            var index = QueryResult.IndexOf(SelectedItem);
            return index > 0;
        }
        #endregion

        #region Filters
        public FilterCollectionViewModel FilterCollectionViewModel { get; private set; }
        
        private void InitializeFilterCollectionViewModel()
        {
            FilterCollectionViewModel = new FilterCollectionViewModel(QuerySession.Filters);
        }

        #region Command: Execute query
        private AsyncRelayCommand _executeQueryCommand;
        public AsyncRelayCommand ExecuteQueryCommand => _executeQueryCommand ?? (_executeQueryCommand = new AsyncRelayCommand(ExecuteQuery));
        private async Task ExecuteQuery()
        {
            await QuerySession.ExecuteQuery();
            SelectedItem = QueryResult.FirstOrDefault();
        }
        #endregion
        #endregion

        #region Slideshow
        private RelayCommand _startSlideShowCommand;
        public RelayCommand StartSlideShowCommand
            => _startSlideShowCommand ?? (_startSlideShowCommand = new RelayCommand(StartSlideShow));
        public void StartSlideShow()
        {
            if(SlideShowViewModel == null)
                SlideShowViewModel = new SlideShowViewModel(this);
            SlideShowActive = true;
            SlideShowViewModel.Start();
        }

        private RelayCommand _closeSlideShowCommand;
        public RelayCommand CloseSlideShowCommand => _closeSlideShowCommand ?? (_closeSlideShowCommand = new RelayCommand(CloseSlideShow));
        private void CloseSlideShow()
        {
            SlideShowViewModel.Stop();
            SlideShowActive = false;
        }

        private bool _slideShowActive;
        public bool SlideShowActive
        {
            get { return _slideShowActive; }
            set { SetValue(ref _slideShowActive, value); }
        }

        private SlideShowViewModel _slideShowViewModel;
        public SlideShowViewModel SlideShowViewModel
        {
            get { return _slideShowViewModel; }
            set { SetValue(ref _slideShowViewModel, value); }
        }
        #endregion
    }
}
