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

namespace MediaCenter.Sessions.Query
{
    public class QuerySessionViewModel : SessionViewModelBase
    {
        private ObservableCollection<string> _availableTags;
        private MediaItem _previousSelectedItem = null;


        public QuerySessionViewModel(SessionBase session) : base(session)
        {
            InitialzeFilterNames();
            InitializeAllTags();
        }

        public override string Name => "Query";

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
        
        private void SelectedItemChanging()
        {
            // stor current selected item, so that SelectedItemChanged can do the saving and cleanup
            _previousSelectedItem = SelectedItem;
        }

        private async Task SelectedItemChanged()
        {
            // start fetching the new content
            Task contentTask = null;
            if (SelectedItem != null)
            {
                Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt ss.fff")} | SelectedItemChanged Requesting{SelectedItem.Name}");
                contentTask = QuerySession.ContentRequested(SelectedItem.Name);
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
                await contentTask;
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
            var index = QueryResult.IndexOf(SelectedItem);
            await QuerySession.DeleteItem(SelectedItem);
            if (QueryResult.Count > index)
                SelectedItem = QueryResult[index]; // show next item, now at the same index of deleted item
            else if (QueryResult.Any())
                SelectedItem = QueryResult.Last(); // we deleted the last item, show the current last item
            else
                SelectedItem = null; // the list is empty now as we deleted the only element, show nothing
        }
        private bool CanExecuteDeleteCurrentImage()
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
        }

        public bool CanExecuteCopyTagFromPrevious()
        {
            if (SelectedItem == null)
                return false;
            var index = QueryResult.IndexOf(SelectedItem);
            return index > 0;
        }
        #endregion

        #region Query
        public ObservableCollection<Filter> Filters => QuerySession.Filters;
        public List<string> FilterNames { get; private set; }

        private string _selectedFilterName;
        public string SelectedFilterName
        {
            get { return _selectedFilterName; }
            set { SetValue(ref _selectedFilterName, value); }
        }
        private void InitialzeFilterNames()
        {
            FilterNames = new List<string> { DateTakenFilter.Name, TagFilter.Name, FavoriteFilter.Name, DateAddedFilter.Name, PrivateFilter.Name };
        }

        #region Command: Add filter
        private RelayCommand _addFilterCommand;
        public RelayCommand AddFilterCommand => _addFilterCommand ?? (_addFilterCommand = new RelayCommand(AddFilter));
        private void AddFilter()
        {
            if (SelectedFilterName == DateTakenFilter.Name)
                Filters.Add(new DateTakenFilter());
            else if (SelectedFilterName == TagFilter.Name)
                Filters.Add(new TagFilter());
            else if (SelectedFilterName == FavoriteFilter.Name)
                Filters.Add(new FavoriteFilter());
            else if (SelectedFilterName == DateTakenFilter.Name)
                Filters.Add(new DateAddedFilter());
            else if (SelectedFilterName == PrivateFilter.Name)
                Filters.Add(new PrivateFilter());

        }
        #endregion

        #region Command: Remove filter
        private RelayCommand<Filter> _removeFilterCommand;
        public RelayCommand<Filter> RemoveFilterCommand => _removeFilterCommand ?? (_removeFilterCommand = new RelayCommand<Filter>(RemoveFilter));
        private void RemoveFilter(Filter filter)
        {
            Filters.Remove(filter);
        }
        #endregion

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
