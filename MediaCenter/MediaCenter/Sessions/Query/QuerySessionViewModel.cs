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
            // stor current selected item, so that SelectedItemChanging can do the saving and cleanup
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
                saveTask = QuerySession.SaveItem(_previousSelectedItem);
            }
            
            // Setup tags
            AvailableTags = SelectedItem != null 
                ? new ObservableCollection<string>(AllTags.Where(x => !SelectedItem.Tags.Contains(x))) 
                : new ObservableCollection<string>();

            // wait for the new content
            if (contentTask != null)
            {
                Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt ss.fff")} | SelectedItemChanged Awaiting {SelectedItem.Name}");
                await contentTask;
                Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt ss.fff")} | SelectedItemChanged Received {SelectedItem.Name}");
                await contentTask;
            }

            // wait for the saving task and then cleanup the content of the previous item
            {
                if (saveTask != null)
                {
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
        
        #endregion

        #region Tags

        public ObservableCollection<string> AllTags { get; private set; }
        private void InitializeAllTags()
        {
            AllTags = new ObservableCollection<string>();
            foreach (var tag in QuerySession.Repository.Tags.OrderBy(s => s))
            {
                AllTags.Add(tag);
            }
        }

        public ObservableCollection<string> AvailableTags
        {
            get { return _availableTags; }
            set { SetValue(ref _availableTags, value); }
        }

        private RelayCommand<string> _addTagCommand;
        public RelayCommand<string> AddTagCommand => _addTagCommand ?? (_addTagCommand = new RelayCommand<string>(AddTag));
        private void AddTag(string newTag)
        {
            SelectedItem.Tags.Add(newTag);
            AvailableTags.Remove(newTag);
            SelectedItem.IsInfoDirty = true;
        }

        private RelayCommand<string> _removeTagCommand;
        public RelayCommand<string> RemoveTagCommand => _removeTagCommand ?? (_removeTagCommand = new RelayCommand<string>(RemoveTag));
        private void RemoveTag(string tag)
        {
            SelectedItem.Tags.Remove(tag);
            AvailableTags.Add(tag);
            SelectedItem.IsInfoDirty = true;
        }

        private RelayCommand _addNewTagCommand;
        public RelayCommand AddNewTagCommand => _addNewTagCommand ?? (_addNewTagCommand = new RelayCommand(AddNewTag));
        private void AddNewTag()
        {
            SelectedItem.Tags.Add(NewTag);
            AllTags.Add(NewTag);
            NewTag = "";
            SelectedItem.IsInfoDirty = true;
        }

        private string _newTag;
        public string NewTag
        {
            get { return _newTag; }
            set { SetValue(ref _newTag,value); }
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
            FilterNames = new List<string> { DateTakenFilter.Name, TagFilter.Name, FavoriteFilter.Name, DateAddedFilter.Name };
        }

        #region Command: Add filter
        private RelayCommand _addFilterCommand;
        public RelayCommand AddFilterCommand => _addFilterCommand ?? (_addFilterCommand = new RelayCommand(AddFilter));
        private void AddFilter()
        {
            if(SelectedFilterName == DateTakenFilter.Name)
                Filters.Add(new DateTakenFilter());
            else if(SelectedFilterName == TagFilter.Name)
                Filters.Add(new TagFilter());
            else if(SelectedFilterName == FavoriteFilter.Name)
                Filters.Add(new FavoriteFilter());
            else if(SelectedFilterName == DateTakenFilter.Name)
                Filters.Add(new DateAddedFilter());

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
    }
}
