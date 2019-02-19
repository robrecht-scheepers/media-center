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
        private RelayCommand<MediaItem> _selectForDetailViewCommand;
        private MediaItemViewModel _detailItem;

        public QuerySessionViewModel(IWindowService windowService, IRepository repository) : base(null, windowService)
        {
            _repository = repository;
            InitializeViewModesList();
            
            FilterCollection = new FilterCollectionViewModel(_repository.Tags);
            FilterCollection.FilterChanged += async (sender, args) => await UpdateMatchCount();
            UpdateMatchCount().Wait();

            DetailItem = new MediaItemViewModel(_repository);

            QueryResultViewModel = new QueryResultViewModel(_repository);
            QueryResultViewModel.SelectionChanged += async (s,a) =>
            {
                await QueryResultViewModelOnSelectionChanged(s,a);
            };
        }

        public override string Name => "View media";

        public FilterCollectionViewModel FilterCollection { get; }

        public QueryResultViewModel QueryResultViewModel { get; }

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
            MatchCount = await _repository.GetQueryCount(FilterCollection.Filters);
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
            set => SetValue(ref _selectedViewMode, value, SelectedViewModeChanged);
        }

        private void SelectedViewModeChanged()
        {
            if (SelectedViewMode == ViewMode.List)
                DetailItem.Load(null).Wait();
            else
                DetailItem.Load(QueryResultViewModel.SelectedItems.FirstOrDefault()).Wait();
        }

        private async Task QueryResultViewModelOnSelectionChanged(object sender, EventArgs args)
        {
            EditMediaInfoViewModel = QueryResultViewModel.SelectedItems.Count > 0
                ? new EditMediaInfoViewModel(QueryResultViewModel.SelectedItems.ToList(), _repository, true)
                : null;

            if (SelectedViewMode != ViewMode.List)
                await DetailItem.Load(QueryResultViewModel.SelectedItems.FirstOrDefault());
        }



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
            await QueryResultViewModel.LoadQueryResult(await _repository.GetQueryItems(FilterCollection.Filters));
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
