using System;
using System.Linq;
using System.Threading.Tasks;
using MediaCenter.MVVM;
using MediaCenter.Sessions.Filters;
using MediaCenter.Sessions.Slideshow;
using System.Windows.Forms;
using MediaCenter.Media;
using MediaCenter.Repository;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using System.Collections.Generic;
using System.IO;
using MediaCenter.Helpers;

namespace MediaCenter.Sessions.Query
{
    public class QuerySessionViewModel : SessionViewModelBase
    {
        private EditMediaInfoViewModel _editMediaInfoViewModel;
        private ViewMode _selectedViewMode;
        private AsyncRelayCommand _startSlideShowCommand;
        private int _matchCount;
        private ViewMode _viewModeBeforeSlideShow;
        private AsyncRelayCommand _deleteCurrentSelectionCommand;
        private AsyncRelayCommand _saveCurrentSelectionToFileCommand;
        private AsyncRelayCommand _executeQueryCommand;
        private readonly ShortcutService _shortcutService;
        private AsyncRelayCommand<MediaItem> _selectForDetailViewCommand;
        private MediaItemViewModel _detailItem;
        private AsyncRelayCommand _switchViewModeToDetailCommand;
        private AsyncRelayCommand _switchViewModeToGridCommand;
        private QueryToolWindowState _toolWindowState;
        private bool _filterWindowIsVisible;
        private bool _propertyWindowIsVisible;
        private bool _toolWindowStateProcessingInProgress;

        public QuerySessionViewModel(IWindowService windowService, IRepository repository, ShortcutService shortcutService, bool readOnly) : base(repository, windowService, shortcutService)
        {
            ReadOnly = readOnly;
            _shortcutService = shortcutService;
            InitializeViewModesList();
            
            FilterCollection = new FilterCollectionViewModel(Repository.Tags);
            FilterCollection.FilterChanged += async (sender, args) => await UpdateMatchCount();
            UpdateMatchCount().Wait();

            DetailItem = new MediaItemViewModel(Repository);

            QueryResultViewModel = new QueryResultViewModel(Repository, _shortcutService);
            QueryResultViewModel.SelectionChanged += async (s,a) =>
            {
                await QueryResultViewModelOnSelectionChanged(s,a);
            };

            EditMediaInfoViewModel = new EditMediaInfoViewModel(Repository, ShortcutService, true, ReadOnly);

            ToolWindowState = QueryToolWindowState.Filters;
        }

        public bool ReadOnly { get; set; }

        public override string Name => "View media";

        public FilterCollectionViewModel FilterCollection { get; }

        public QueryResultViewModel QueryResultViewModel { get; }

        public SlideShowViewModel SlideShowViewModel {get; set; }

        
        public MediaItemViewModel DetailItem
        {
            get => _detailItem;
            set => SetValue(ref _detailItem, value);
        }

        public int MatchCount
        {
            get => _matchCount;
            set => SetValue(ref _matchCount, value);
        }
        private async Task UpdateMatchCount()
        {
            MatchCount = await Repository.GetQueryCount(FilterCollection.Filters);
        }
        
        public EditMediaInfoViewModel EditMediaInfoViewModel
        {
            get => _editMediaInfoViewModel;
            set => SetValue(ref _editMediaInfoViewModel, value);
        }

        public QueryToolWindowState ToolWindowState
        {
            get => _toolWindowState;
            set => SetValue(ref _toolWindowState, value, ToolWindowStateChanged);
        }

        public bool FilterWindowIsVisible
        {
            get => _filterWindowIsVisible;
            set => SetValue(ref _filterWindowIsVisible, value, FilterVisibilityChanged);
        }

        public bool PropertyWindowIsVisible
        {
            get => _propertyWindowIsVisible;
            set => SetValue(ref _propertyWindowIsVisible, value, PropertyVisibilityChanged);
        }

        public List<ViewMode> ViewModesList { get; private set; }
        private void InitializeViewModesList()
        {
            ViewModesList = new List<ViewMode> { ViewMode.Detail, ViewMode.Grid };
        }
        public ViewMode SelectedViewMode
        {
            get => _selectedViewMode;
            set => SetValue(ref _selectedViewMode, value);
        }

        public AsyncRelayCommand SwitchViewModeToDetailCommand =>
            _switchViewModeToDetailCommand ?? (_switchViewModeToDetailCommand =
                new AsyncRelayCommand(SwitchViewModeToDetail, CanExecuteSwitchViewModeToDetail));
        private bool CanExecuteSwitchViewModeToDetail()
        {
            return SelectedViewMode != ViewMode.Detail;
        }
        private async Task SwitchViewModeToDetail()
        {
            SelectedViewMode = ViewMode.Detail;
            await DetailItem.Load(QueryResultViewModel.SelectedItems.FirstOrDefault());
        }

        public AsyncRelayCommand SwitchViewModeToGridCommand =>
            _switchViewModeToGridCommand ?? (_switchViewModeToGridCommand =
                new AsyncRelayCommand(SwitchViewModeToGrid, CanExecuteSwitchViewModeToGrid));
        private bool CanExecuteSwitchViewModeToGrid()
        {
            return SelectedViewMode != ViewMode.Grid;
        }
        private async Task SwitchViewModeToGrid()
        {
            SelectedViewMode = ViewMode.Grid;
            await DetailItem.Load(null);
        }

        private async Task QueryResultViewModelOnSelectionChanged(object sender, EventArgs args)
        {
            EditMediaInfoViewModel.LoadItems(QueryResultViewModel.SelectedItems.ToList());

            if (SelectedViewMode != ViewMode.Grid)
                await DetailItem.Load(QueryResultViewModel.SelectedItems.FirstOrDefault());
        }
        

        public AsyncRelayCommand<MediaItem> SelectForDetailViewCommand =>
            _selectForDetailViewCommand ??
            (_selectForDetailViewCommand = new AsyncRelayCommand<MediaItem>(SelectForDetailView));
        private async Task SelectForDetailView(MediaItem item)
        {
            SelectedViewMode = ViewMode.Detail;
            await DetailItem.Load(item);
        }

        public AsyncRelayCommand DeleteCurrentSelectionCommand
            => _deleteCurrentSelectionCommand ?? (_deleteCurrentSelectionCommand = new AsyncRelayCommand(DeleteCurrentSelection, CanExecuteDeleteCurrentSelection));
        private async Task DeleteCurrentSelection()
        {
            if (WindowService.AskConfirmation("Are you sure you want to delete the selected items? This action cannot be undone."))
            {
                foreach (var item in QueryResultViewModel.SelectedItems.ToList())
                {
                    QueryResultViewModel.RemoveItem(item);
                    await Repository.DeleteItem(item);
                }    
            }
        }
        private bool CanExecuteDeleteCurrentSelection()
        {
            return (!ReadOnly && QueryResultViewModel != null && QueryResultViewModel.SelectedItems.Count > 0);
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

            WindowService.ShowMessage(message,"Save success");
        }
        private bool CanExecuteSaveCurrentSelectionToFile()
        {
            return (QueryResultViewModel != null && QueryResultViewModel.SelectedItems.Count > 0);
        }
        
        public AsyncRelayCommand ExecuteQueryCommand => _executeQueryCommand ?? (_executeQueryCommand = new AsyncRelayCommand(ExecuteQuery, CanExecuteQuery));

        private bool CanExecuteQuery()
        {
            return MatchCount > 0;
        }

        private async Task ExecuteQuery()
        {
            await QueryResultViewModel.LoadQueryResult(await Repository.GetQueryItems(FilterCollection.Filters));
            ToolWindowState = QueryToolWindowState.Hidden;
        }

        public AsyncRelayCommand StartSlideShowCommand => _startSlideShowCommand ?? (_startSlideShowCommand = new AsyncRelayCommand(StartSlideShow));
        public async Task StartSlideShow()
        {
            _viewModeBeforeSlideShow = SelectedViewMode;
            if(SelectedViewMode == ViewMode.Grid)
                await SwitchViewModeToDetail();
            SelectedViewMode = ViewMode.SlideShow;
            SlideShowViewModel = new SlideShowViewModel(this, WindowService);
            SlideShowViewModel.WindowId = WindowService.OpenWindow(SlideShowViewModel,false, OnSlideShowClosed);
            SlideShowViewModel.Start();
        }
        private void OnSlideShowClosed()
        {
            SlideShowViewModel.Stop();
            SlideShowViewModel = null;
            if (_viewModeBeforeSlideShow == ViewMode.Grid)
                SwitchViewModeToGrid().Wait();
            else
                SwitchViewModeToDetail().Wait();
        }

        private void FilterVisibilityChanged()
        {
            if(_toolWindowStateProcessingInProgress)
                return;
            ToolWindowState = FilterWindowIsVisible ? QueryToolWindowState.Filters : QueryToolWindowState.Hidden;
            
        }

        private void PropertyVisibilityChanged()
        {
            if (_toolWindowStateProcessingInProgress)
                return;
            ToolWindowState = PropertyWindowIsVisible ? QueryToolWindowState.Properties : QueryToolWindowState.Hidden;
        }

        private void ToolWindowStateChanged()
        {
            _toolWindowStateProcessingInProgress = true;
            switch (ToolWindowState)
            {
                case QueryToolWindowState.Hidden:
                    FilterWindowIsVisible = false;
                    PropertyWindowIsVisible = false;
                    break;
                case QueryToolWindowState.Filters:
                    FilterWindowIsVisible = true;
                    PropertyWindowIsVisible = false;
                    break;
                case QueryToolWindowState.Properties:
                    FilterWindowIsVisible = false;
                    PropertyWindowIsVisible = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _toolWindowStateProcessingInProgress = false;
        }
    }
}
