using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaCenter.Media;
using MediaCenter.MVVM;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Configuration;
using MediaCenter.Helpers;

namespace MediaCenter.Sessions.Staging
{
    public class StagingSessionViewModel : SessionViewModelBase
    {
        private EditMediaInfoViewModel _editMediaInfoViewModel;

        public StagingSessionViewModel(StagingSession session, IWindowService windowService) : base(session, windowService)
        {
            SelectedItems = new BatchObservableCollection<MediaItem>();
            SelectedItems.CollectionChanged += SelectedItemsOnCollectionChanged;
        }

        public override string Name => "Add media";

        public StagingSession StagingSession => (StagingSession)Session;

        public BatchObservableCollection<MediaItem> SelectedItems { get; }
        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            EditMediaInfoViewModel = SelectedItems.Any() 
                ? new EditMediaInfoViewModel(SelectedItems.ToList(), StagingSession.Repository, false) 
                : null;
        }

        public EditMediaInfoViewModel EditMediaInfoViewModel
        {
            get { return _editMediaInfoViewModel; }
            set { SetValue(ref _editMediaInfoViewModel, value); }
        }

        public StagedItem PreviewItem
        {
            get { return _previewItem; }
            set { SetValue(ref _previewItem, value); }
        }

        #region Command: Show preview

        private RelayCommand<StagedItem> _showPreviewCommand;
        public RelayCommand<StagedItem> ShowPreviewCommand => _showPreviewCommand ?? (_showPreviewCommand = new RelayCommand<StagedItem>(ShowPreview));
        private void ShowPreview(StagedItem item)
        {
            PreviewItem = item;
        }

        #endregion

        #region Edit item
        private EditStagedItemViewModel _editViewModel;
        public EditStagedItemViewModel EditViewModel
        {
            get { return _editViewModel; }
            set { SetValue(ref _editViewModel, value); }
        }

        private bool _showEditViewModel;
        public bool ShowEditViewModel
        {
            get { return _showEditViewModel; }
            set { SetValue(ref _showEditViewModel, value); }
        }

        private RelayCommand<object> _beginEditItemCommand;
        public RelayCommand<object> BeginEditItemCommand
        {
            get { return _beginEditItemCommand ?? (_beginEditItemCommand = new RelayCommand<object>(BeginEditStagedItem)); }
        }
        public void BeginEditStagedItem(object items)
        {
            var list = ((System.Collections.IList)items).Cast<StagedItem>().ToList();

            if(list.Count == 0)
                return;

            EditViewModel = new EditStagedItemViewModel(list);
            EditViewModel.CloseRequested += EditViewModelOnCloseRequested;
            ShowEditViewModel = true;
        }
        private void EditViewModelOnCloseRequested(object sender, CloseEditViewModelEventArgs args)
        {
            if (args.CloseType == EditViewModelCloseType.Save)
            {
                var editViewModel = (EditStagedItemViewModel)sender;
                var i = 0;
                foreach (var item in editViewModel.Items)
                {
                    var newDate = editViewModel.NewDateTaken.AddSeconds(i++);
                    StagingSession.EditStagedItemDate(item, newDate);
                }
            }
            EditViewModel.CloseRequested -= EditViewModelOnCloseRequested;
            EditViewModel = null;
            ShowEditViewModel = false;
        }
        #endregion

        #region Command: Add items
        private AsyncRelayCommand _addMediaCommand;
        public AsyncRelayCommand AddMediaCommand => _addMediaCommand ?? (_addMediaCommand = new AsyncRelayCommand(AddMedia));
        private async Task AddMedia()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Select the media files to be added",
                Filter = "Media Files(*.BMP;*.JPG;*.JPEG;*.PNG;*.MP4;*.AVI;*.MTS;*.M4V)|*.BMP;*.JPG;*.JPEG;*.PNG;*.MP4;*.AVI;*.MTS;*.M4V"
            };

            dialog.ShowDialog();
            var selectedImages = dialog.FileNames;
            if (!selectedImages.Any())
                return;
            await StagingSession.AddMediaItems(selectedImages);
        }
        #endregion

        #region Command: add folder
        private AsyncRelayCommand _addDirectoryCommand;
        public AsyncRelayCommand AddDirectoryCommand => _addDirectoryCommand ?? (_addDirectoryCommand = new AsyncRelayCommand(AddDirectory));
        private async Task AddDirectory()
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();

            var selectedFolder = dialog.SelectedPath;
            if (string.IsNullOrEmpty(selectedFolder))
                return;

            // TODO: make this configurable in the dialog, now global via config file
            SearchOption searchOption = SearchOption.AllDirectories;
            if (ConfigurationManager.AppSettings["LoadSubDirs"].ToLower() == "false")
                searchOption = SearchOption.TopDirectoryOnly;

            await StagingSession.AddMediaItems(Directory.GetFiles(selectedFolder, "*.*", searchOption));
        }
        #endregion
        
        #region Command: Remove selected items
        
        private RelayCommand _removeItemsCommand;
        public RelayCommand RemoveItemsCommand => _removeItemsCommand ?? (_removeItemsCommand = new RelayCommand(RemoveItems));

        private void RemoveItems()
        {
            foreach (var item in SelectedItems.Cast<StagedItem>().ToList())
            {
                StagingSession.RemoveStagedItem(item);
            }
        }
        #endregion

        #region Command: save staged images to repository
        private AsyncRelayCommand _saveToRepositoryCommand;
        private StagedItem _previewItem;
        public AsyncRelayCommand SaveToRepositoryCommand => _saveToRepositoryCommand ?? (_saveToRepositoryCommand = new AsyncRelayCommand(SaveToRepository,CanExecuteSaveToRepository));
        private bool CanExecuteSaveToRepository()
        {
            return StagingSession.StagedItems.Any();
        }
        private async Task SaveToRepository()
        {
            await StagingSession.SaveToRepository();
        }
        #endregion
    }
}
