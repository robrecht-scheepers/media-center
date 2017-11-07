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

namespace MediaCenter.Sessions.Query
{
    public class QuerySessionViewModel : SessionViewModelBase
    {
        private SlideShowViewModel _slideShowViewModel;
        private QueryResultViewModel _queryResultViewModel;
        private EditMediaInfoViewModel _editMediaInfoViewModel;
        private QueryResultViewMode _resultViewMode;

        public QuerySessionViewModel(SessionBase session) : base(session)
        {
            InitializeFilterCollectionViewModel();
            ResultViewModesList = new List<QueryResultViewMode>{QueryResultViewMode.Detail, QueryResultViewMode.List};
        }

        public override string Name => "View media";

        public QuerySession QuerySession => (QuerySession) Session;

        public IRepository Repository => Session.Repository;

        public FilterCollectionViewModel FilterCollectionViewModel { get; private set; }
        private void InitializeFilterCollectionViewModel()
        {
            FilterCollectionViewModel = new FilterCollectionViewModel(QuerySession.Filters, Repository.Tags);
        }

        public QueryResultViewMode ResultViewMode
        {
            get { return _resultViewMode; }
            set { SetValue(ref _resultViewMode, value, ResultViewModeChanged); }
        }

        private void ResultViewModeChanged()
        {
            if(QueryResultViewModel != null)
                InitializeQueryResultViewModel();
        }

        public List<QueryResultViewMode> ResultViewModesList { get; }
        public QueryResultViewModel QueryResultViewModel
        {
            get { return _queryResultViewModel; }
            set { SetValue(ref _queryResultViewModel, value); }
        }
        private void InitializeQueryResultViewModel()
        {
            if(QueryResultViewModel != null)
                QueryResultViewModel.SelectionChanged -= QueryResultViewModelOnSelectionChanged;

            QueryResultViewModel = ResultViewMode == QueryResultViewMode.Detail 
                ? (QueryResultViewModel)new QueryResultDetailViewModel(QuerySession.QueryResult, Repository)
                : (QueryResultViewModel)new QueryResultListViewModel(QuerySession.QueryResult);
            QueryResultViewModel.SelectionChanged += QueryResultViewModelOnSelectionChanged;
        }
        private async void QueryResultViewModelOnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            EditMediaInfoViewModel?.PublishToItems();
            EditMediaInfoViewModel = QueryResultViewModel.SelectedItems.Count > 0
                ? new EditMediaInfoViewModel(QueryResultViewModel.SelectedItems.ToList(), Repository.Tags.ToList())
                : null;
            foreach (var dirtyItem in args.ItemsRemoved.Where(x => x.IsDirty))
            {
                await Repository.SaveItem(dirtyItem.Name);
            }
        }

        public EditMediaInfoViewModel EditMediaInfoViewModel
        {
            get { return _editMediaInfoViewModel; }
            set { SetValue(ref _editMediaInfoViewModel, value); }
        }

        #region Command: delete current selection
        private AsyncRelayCommand _deleteCurrentSelectionCommand;
        public AsyncRelayCommand DeleteCurrentSelectionCommand
            => _deleteCurrentSelectionCommand ?? (_deleteCurrentSelectionCommand = new AsyncRelayCommand(DeleteCurrentSelection, CanExecuteDeleteCurrentSelection));
        private async Task DeleteCurrentSelection()
        {
            var confirmationResult = MessageBox.Show("Are you sure you want to delete the selected items from the repository? This action cannot be undone.", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Hand);
            if (confirmationResult == MessageBoxResult.Yes)
            {
                foreach (var item in QueryResultViewModel.SelectedItems)
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

            if (QueryResultViewModel.SelectedItems.Count == 1)
            {
                var item = QueryResultViewModel.SelectedItems.First();
                var dialog = new SaveFileDialog()
                {
                    FileName = item.ContentFileName
                };
                dialog.ShowDialog();
                if (string.IsNullOrEmpty(dialog.FileName))
                    return;
                {
                    await Repository.SaveContentToFile(item, dialog.FileName);
                }
            }
            else
            {
                var dialog = new FolderBrowserDialog()
                {
                    Description = $"Select the destination folder for saving the selected {QueryResultViewModel.SelectedItems.Count} items"
                };
                dialog.ShowDialog();
                var selectedFolder = dialog.SelectedPath;
                if (string.IsNullOrEmpty(selectedFolder))
                    return;
                await Repository.SaveContentToFolder(QueryResultViewModel.SelectedItems.ToList(), selectedFolder);
            }
        }
        private bool CanExecuteSaveCurrentSelectionToFile()
        {
            return (QueryResultViewModel != null && QueryResultViewModel.SelectedItems.Count > 0);
        }
        #endregion
        
        private AsyncRelayCommand _executeQueryCommand;
        public AsyncRelayCommand ExecuteQueryCommand => _executeQueryCommand ?? (_executeQueryCommand = new AsyncRelayCommand(ExecuteQuery));
        private async Task ExecuteQuery()
        {
            await QuerySession.ExecuteQuery();
            InitializeQueryResultViewModel();
        }
        
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

        

        public SlideShowViewModel SlideShowViewModel
        {
            get { return _slideShowViewModel; }
            set { SetValue(ref _slideShowViewModel, value); }
        }
        #endregion
    }
}
