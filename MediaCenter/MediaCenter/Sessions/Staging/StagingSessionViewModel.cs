using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaCenter.Media;
using MediaCenter.MVVM;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Configuration;
using MediaCenter.Sessions.Tags;

namespace MediaCenter.Sessions.Staging
{
    public class StagingSessionViewModel : SessionViewModelBase
    {
        public StagingSessionViewModel(StagingSession session) : base(session)
        {
            InitializeTagsViewModel();

        }

        public override string Name => "Add images";

        public StagingSession StagingSession => (StagingSession)Session;

        private TagsViewModel _tagsViewModel;
        public TagsViewModel TagsViewModel
        {
            get { return _tagsViewModel; }
            set { SetValue(ref _tagsViewModel, value); }
        }

        

        private void InitializeTagsViewModel()
        {
            TagsViewModel = new TagsViewModel(Session.Repository.Tags);
        }

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

        private RelayCommand<StagedItem> _beginEditItemCommand;
        public RelayCommand<StagedItem> BeginEditItemCommand
        {
            get { return _beginEditItemCommand ?? (_beginEditItemCommand = new RelayCommand<StagedItem>(BeginEditStagedItem)); }
        }
        public void BeginEditStagedItem(StagedItem item)
        {
            EditViewModel = new EditStagedItemViewModel(item);
            EditViewModel.CloseRequested += EditViewModelOnCloseRequested;
            ShowEditViewModel = true;
        }
        private void EditViewModelOnCloseRequested(object sender, CloseEditViewModelEventArgs args)
        {
            if (args.CloseType == EditViewModelCloseType.Save)
            {
                var editViewModel = (EditStagedItemViewModel)sender;
                StagingSession.EditStagedItemDate(editViewModel.MediaItem, editViewModel.NewDateTaken);
            }
            EditViewModel.CloseRequested -= EditViewModelOnCloseRequested;
            EditViewModel = null;
            ShowEditViewModel = false;
        }
        #endregion

        #region Command: Add image files 
        private AsyncRelayCommand _addMediaCommand;
        public AsyncRelayCommand AddMediaCommand => _addMediaCommand ?? (_addMediaCommand = new AsyncRelayCommand(AddMedia));
        private async Task AddMedia()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Select the media files to be added",
                Filter = "Media Files(*.BMP;*.JPG;*.JPEG;*.PNG;*.MP4;*.AVI;*.MTS)|*.BMP;*.JPG;*.JPEG;*.PNG;*.MP4;*.AVI;*.MTS"
            };

            dialog.ShowDialog();
            var selectedImages = dialog.FileNames;
            if (!selectedImages.Any())
                return;
            await StagingSession.AddMediaItems(selectedImages);
        }
        #endregion

        #region Command: add image folder
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
        
        #region Command: Remove staged item
        private RelayCommand<StagedItem> _removeItemCommand;
        public RelayCommand<StagedItem> RemoveItemCommand => _removeItemCommand ?? (_removeItemCommand = new RelayCommand<StagedItem> (RemoveItem));

        private void RemoveItem(StagedItem item)
        {
            StagingSession.RemoveStagedItem(item);
        }
        #endregion
        
        #region Command: save staged images to repository
        private AsyncRelayCommand _saveToRepositoryCommand;
        public AsyncRelayCommand SaveToRepositoryCommand => _saveToRepositoryCommand ?? (_saveToRepositoryCommand = new AsyncRelayCommand(SaveToRepository,CanExecuteSaveToRepository));
        private bool CanExecuteSaveToRepository()
        {
            return StagingSession.StagedItems.Any();
        }
        private async Task SaveToRepository()
        {
            if(TagsViewModel.SelectedTags.Any())
                await StagingSession.SaveToRepository(TagsViewModel.SelectedTags);
            else
                await StagingSession.SaveToRepository();
            InitializeTagsViewModel();
        }
        #endregion

        
    }
}
