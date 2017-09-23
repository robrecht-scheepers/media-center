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
        private RelayCommand<MediaItem> _removeItemCommand;
        public RelayCommand<MediaItem> RemoveItemCommand => _removeItemCommand ?? (_removeItemCommand = new RelayCommand<MediaItem> (RemoveItem));

        private void RemoveItem(MediaItem item)
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
