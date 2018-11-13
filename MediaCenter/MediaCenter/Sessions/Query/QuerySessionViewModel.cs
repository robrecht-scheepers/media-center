using System;
using System.Linq;
using System.Threading.Tasks;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Filters;
using MediaCenter.Sessions.Slideshow;
using System.Windows;
using System.Windows.Forms;
using MediaCenter.Media;
using MediaCenter.Repository;
using MessageBox = System.Windows.MessageBox;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using System.Collections.Generic;
using System.IO;
using MediaCenter.Helpers;

namespace MediaCenter.Sessions.Query
{
    public class QuerySessionViewModel : SessionViewModelBase
    {
        public enum ViewMode { List, Detail, SlideShow }
    
        private SlideShowViewModel _slideShowViewModel;
        private QueryResultViewModel _queryResultViewModel;
        private EditMediaInfoViewModel _editMediaInfoViewModel;
        private ViewMode _selectedResultViewMode;
        private RelayCommand _startSlideShowCommand;
        private int _matchCount;
        private RelayCommand _closeSlideShowCommand;
        private ViewMode _viewModeBeforeSlideshow;

        public QuerySessionViewModel(SessionBase session, IWindowService windowService) : base(session, windowService)
        {
            InitializeResultViewModesList();
            InitializeFilterCollectionViewModel();
            UpdateMatchCount().Wait();
        }

        public override string Name => "View media";

        public QuerySession QuerySession => (QuerySession) Session;

        public IRepository Repository => Session.Repository;

        public int MatchCount
        {
            get => _matchCount;
            set => SetValue(ref _matchCount, value);
        }

        public FilterCollectionViewModel FilterCollectionViewModel { get; private set; }
        private void InitializeFilterCollectionViewModel()
        {
            FilterCollectionViewModel = new FilterCollectionViewModel(QuerySession.Filters, Repository.Tags);
            FilterCollectionViewModel.FilterChanged += async (sender, args) => await UpdateMatchCount();
        }
        
        private async Task UpdateMatchCount()
        {
            MatchCount = await QuerySession.CalculateMatchCount();
        }

        public EditMediaInfoViewModel EditMediaInfoViewModel
        {
            get => _editMediaInfoViewModel;
            set => SetValue(ref _editMediaInfoViewModel, value);
        }

        public List<ViewMode> ViewModesList { get; private set; }
        private void InitializeResultViewModesList()
        {
            ViewModesList = new List<ViewMode> { ViewMode.Detail, ViewMode.List };
        }
        public ViewMode SelectedResultViewMode
        {
            get => _selectedResultViewMode;
            set => SetValue(ref _selectedResultViewMode, value, SelectedResultViewModeChanged);
        }
        private void SelectedResultViewModeChanged()
        {
            if(QueryResultViewModel != null)
                InitializeQueryResultViewModel();
        }

        public QueryResultViewModel QueryResultViewModel
        {
            get => _queryResultViewModel;
            set => SetValue(ref _queryResultViewModel, value);
        }
        private void InitializeQueryResultViewModel()
        {
            MediaItem selectedElement = null;

            if (QueryResultViewModel != null)
            {
                QueryResultViewModel.SelectionChanged -= QueryResultViewModelOnSelectionChanged;
                selectedElement = QueryResultViewModel?.SelectedItems.FirstOrDefault();
            }

            switch (SelectedResultViewMode)
            {
                case ViewMode.List:
                    QueryResultViewModel = new QueryResultListViewModel(QuerySession.QueryResult, selectedElement);
                    break;
                case ViewMode.Detail:
                    QueryResultViewModel =
                        new QueryResultDetailViewModel(QuerySession.QueryResult, Repository, selectedElement);
                    break;
                case ViewMode.SlideShow:
                    QueryResultViewModel = new SlideShowViewModel(QuerySession.QueryResult, Repository, selectedElement);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            QueryResultViewModel.SelectionChanged += QueryResultViewModelOnSelectionChanged;
        }
        private async void QueryResultViewModelOnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            EditMediaInfoViewModel = QueryResultViewModel.SelectedItems.Count > 0
                ? new EditMediaInfoViewModel(QueryResultViewModel.SelectedItems.ToList(), Repository,true)
                : null;            
        }

        private RelayCommand<MediaItem> _selectForDetailViewCommand;
        public RelayCommand<MediaItem> SelectForDetailViewCommand =>
            _selectForDetailViewCommand ??
            (_selectForDetailViewCommand = new RelayCommand<MediaItem>(SelectForDetailView));
        private void SelectForDetailView(MediaItem item)
        {
            SelectedResultViewMode = ViewMode.Detail;
        }

        #region Command: delete current selection
        private AsyncRelayCommand _deleteCurrentSelectionCommand;
        public AsyncRelayCommand DeleteCurrentSelectionCommand
            => _deleteCurrentSelectionCommand ?? (_deleteCurrentSelectionCommand = new AsyncRelayCommand(DeleteCurrentSelection, CanExecuteDeleteCurrentSelection));
        private async Task DeleteCurrentSelection()
        {
            var confirmationResult = MessageBox.Show("Are you sure you want to delete the selected items from the repository? This action cannot be undone.", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirmationResult == MessageBoxResult.Yes)
            {
                foreach (var item in QueryResultViewModel.SelectedItems.ToList())
                {
                    await QuerySession.DeleteItem(item);
                }    
            }
        }
        private bool CanExecuteDeleteCurrentSelection()
        {
            return (QueryResultViewModel != null && QueryResultViewModel.SelectedItems.Count > 0);
        }
        #endregion

        #region Command: save current selection to file
        private AsyncRelayCommand _saveCurrentSelectionToFileCommand;
        public AsyncRelayCommand SaveCurrentSelectionToFileCommand
            => _saveCurrentSelectionToFileCommand ?? (_saveCurrentSelectionToFileCommand = new AsyncRelayCommand(SaveCurrentSelectionToFile, CanExecuteSaveCurrentSelectionToFile));
        private async Task SaveCurrentSelectionToFile()
        {
            if (QueryResultViewModel == null || QueryResultViewModel.SelectedItems.Count == 0)
                return;

            string message = "";

            if (QueryResultViewModel.SelectedItems.Count == 1)
            {
                var item = QueryResultViewModel.SelectedItems.First();

                var dialog = new SaveFileDialog() { FileName = item.ContentFileName };
                var dialogResult = dialog.ShowDialog();
                if (!dialogResult.HasValue || !dialogResult.Value || string.IsNullOrEmpty(dialog.FileName))
                    return;
                
                await Repository.SaveContentToFile(item, dialog.FileName);
                message = $"File {Path.GetFileName(dialog.FileName)} was saved successfully.";
            }
            else
            {
                var dialog = new FolderBrowserDialog()
                {
                    Description = $"Select the destination folder for saving the selected {QueryResultViewModel.SelectedItems.Count} items"
                };
                if(dialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(dialog.SelectedPath))
                    return;

                var selectedFolder = dialog.SelectedPath;
                await Repository.SaveMultipleContentToFolder(QueryResultViewModel.SelectedItems.ToList(), selectedFolder);
                message = $"{QueryResultViewModel.SelectedItems.Count} files were saved successfully";
            }

            MessageBox.Show(message,"Save success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private bool CanExecuteSaveCurrentSelectionToFile()
        {
            return (QueryResultViewModel != null && QueryResultViewModel.SelectedItems.Count > 0);
        }
        #endregion

        #region Command: execute query
        private AsyncRelayCommand _executeQueryCommand;
        public AsyncRelayCommand ExecuteQueryCommand => _executeQueryCommand ?? (_executeQueryCommand = new AsyncRelayCommand(ExecuteQuery));
        private async Task ExecuteQuery()
        {
            await QuerySession.ExecuteQuery();
            InitializeQueryResultViewModel();
            foreach (var item in QuerySession.QueryResult)
            {
                item.Thumbnail = await Repository.GetThumbnail(item);
            }
        }
        #endregion

        #region Slideshow

        public SlideShowViewModel SlideShowViewModel => QueryResultViewModel as SlideShowViewModel;

        public RelayCommand StartSlideShowCommand
            => _startSlideShowCommand ?? (_startSlideShowCommand = new RelayCommand(StartSlideShow));
        public void StartSlideShow()
        {
            if (SelectedResultViewMode == ViewMode.SlideShow)
            {
                CloseSlideShow();
            }

            _viewModeBeforeSlideshow = SelectedResultViewMode;
            SelectedResultViewMode = ViewMode.SlideShow;
            InitializeQueryResultViewModel();
            WindowService.OpenWindow(this,false);
            ((SlideShowViewModel)QueryResultViewModel).Start();
        }

        public RelayCommand CloseSlideShowCommand =>
            _closeSlideShowCommand ?? (_closeSlideShowCommand = new RelayCommand(CloseSlideShow));

        private void CloseSlideShow()
        {
            ((SlideShowViewModel)QueryResultViewModel).Stop();
            SelectedResultViewMode = _viewModeBeforeSlideshow;
            InitializeQueryResultViewModel();
        }
        
        #endregion
    }
}
