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
    
        private QueryResultViewModel _queryResultViewModel;
        private EditMediaInfoViewModel _editMediaInfoViewModel;
        private ViewMode _selectedViewMode;
        private RelayCommand _startSlideShowCommand;
        private int _matchCount;
        private RelayCommand _closeSlideShowCommand;
        private ViewMode _viewModeBeforeSlideShow;
        private AsyncRelayCommand _deleteCurrentSelectionCommand;
        private AsyncRelayCommand _saveCurrentSelectionToFileCommand;
        private AsyncRelayCommand _executeQueryCommand;
        private readonly IRepository _repository;

        public QuerySessionViewModel(IWindowService windowService, IRepository repository) : base(null, windowService)
        {
            _repository = repository;
            InitializeViewModesList();
            InitializeFilterCollectionViewModel();
            UpdateMatchCount().Wait();

            QueryResultViewModel = new QueryResultViewModel(_repository);
            QueryResultViewModel.SelectionChanged += QueryResultViewModelOnSelectionChanged;
        }

        public override string Name => "View media";

        public FilterCollectionViewModel Filters { get; private set; }

        private void InitializeFilterCollectionViewModel()
        {
            Filters = new FilterCollectionViewModel(_repository.Tags);
            Filters.FilterChanged += async (sender, args) => await UpdateMatchCount();
        }

        public int MatchCount
        {
            get => _matchCount;
            set => SetValue(ref _matchCount, value);
        }
        private async Task UpdateMatchCount()
        {
            var tmpFiltersList = Filters.FilterViewModels.Select(x => x.Filter).ToList();
            if (!tmpFiltersList.Any(x => x is PrivateFilter))
            {
                tmpFiltersList.Add(new PrivateFilter { PrivateSetting = PrivateFilter.PrivateOption.NoPrivate });
            }

            MatchCount = await _repository.GetQueryCount(tmpFiltersList);
        }
        
        public EditMediaInfoViewModel EditMediaInfoViewModel
        {
            get => _editMediaInfoViewModel;
            set => SetValue(ref _editMediaInfoViewModel, value);
        }

        public List<ViewMode> ViewModesList { get; private set; }
        private void InitializeViewModesList()
        {
            ViewModesList = new List<ViewMode> { ViewMode.Detail, ViewMode.List };
        }
        public ViewMode SelectedViewMode
        {
            get => _selectedViewMode;
            set => SetValue(ref _selectedViewMode, value);
        }

        public QueryResultViewModel QueryResultViewModel
        {
            get => _queryResultViewModel;
            set => SetValue(ref _queryResultViewModel, value);
        }
        
        private void QueryResultViewModelOnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            EditMediaInfoViewModel = QueryResultViewModel.SelectedItems.Count > 0
                ? new EditMediaInfoViewModel(QueryResultViewModel.SelectedItems.ToList(), _repository, true)
                : null;            
        }

        private RelayCommand<MediaItem> _selectForDetailViewCommand;
        public RelayCommand<MediaItem> SelectForDetailViewCommand =>
            _selectForDetailViewCommand ??
            (_selectForDetailViewCommand = new RelayCommand<MediaItem>(SelectForDetailView));
        private void SelectForDetailView(MediaItem item)
        {
            SelectedViewMode = ViewMode.Detail;
        }

        public AsyncRelayCommand DeleteCurrentSelectionCommand
            => _deleteCurrentSelectionCommand ?? (_deleteCurrentSelectionCommand = new AsyncRelayCommand(DeleteCurrentSelection, CanExecuteDeleteCurrentSelection));
        private async Task DeleteCurrentSelection()
        {
            var confirmationResult = MessageBox.Show("Are you sure you want to delete the selected items from the repository? This action cannot be undone.", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirmationResult == MessageBoxResult.Yes)
            {
                foreach (var item in QueryResultViewModel.SelectedItems.ToList())
                {
                    QueryResultViewModel.RemoveItem(item);
                    await _repository.DeleteItem(item);
                }    
            }
        }
        private bool CanExecuteDeleteCurrentSelection()
        {
            return (QueryResultViewModel != null && QueryResultViewModel.SelectedItems.Count > 0);
        }
        

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
                
                await _repository.SaveContentToFile(item, dialog.FileName);
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
                await _repository.SaveMultipleContentToFolder(QueryResultViewModel.SelectedItems.ToList(), selectedFolder);
                message = $"{QueryResultViewModel.SelectedItems.Count} files were saved successfully";
            }

            MessageBox.Show(message,"Save success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private bool CanExecuteSaveCurrentSelectionToFile()
        {
            return (QueryResultViewModel != null && QueryResultViewModel.SelectedItems.Count > 0);
        }
        
        public AsyncRelayCommand ExecuteQueryCommand => _executeQueryCommand ?? (_executeQueryCommand = new AsyncRelayCommand(ExecuteQuery));
        private async Task ExecuteQuery()
        {
            var tmpFiltersList = Filters.FilterViewModels.Select(x => x.Filter).ToList();
            if (!tmpFiltersList.Any(x => x is PrivateFilter))
            {
                tmpFiltersList.Add(new PrivateFilter { PrivateSetting = PrivateFilter.PrivateOption.NoPrivate });
            }

            await QueryResultViewModel.LoadQueryResult(await _repository.GetQueryItems(tmpFiltersList));
        }
       
        
        public SlideShowViewModel SlideShowViewModel => QueryResultViewModel as SlideShowViewModel;

        public RelayCommand StartSlideShowCommand => _startSlideShowCommand ?? (_startSlideShowCommand = new RelayCommand(StartSlideShow));
        public void StartSlideShow()
        {
            _viewModeBeforeSlideShow = SelectedViewMode;
            SelectedViewMode = ViewMode.SlideShow;
            WindowService.OpenWindow(this,false);
            ((SlideShowViewModel)QueryResultViewModel).Start();
        }

        public RelayCommand CloseSlideShowCommand => _closeSlideShowCommand ?? (_closeSlideShowCommand = new RelayCommand(CloseSlideShow));
        private void CloseSlideShow()
        {
            ((SlideShowViewModel)QueryResultViewModel).Stop();
            SelectedViewMode = _viewModeBeforeSlideShow;
        }
        
    }
}
