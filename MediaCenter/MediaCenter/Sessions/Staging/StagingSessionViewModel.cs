using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaCenter.MVVM;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace MediaCenter.Sessions.Staging
{
    public class StagingSessionViewModel : SessionViewModelBase
    {
        public StagingSessionViewModel(StagingSession session) : base(session)
        { }

        public override string Name => "Add images";

        public StagingSession StagingSession => (StagingSession)Session;

        #region Command: Add image files 
        private AsyncRelayCommand _addImagesCommand;
        public AsyncRelayCommand AddImagesCommand => _addImagesCommand ?? (_addImagesCommand = new AsyncRelayCommand(AddImages));
        private async Task AddImages()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Select the image files to be added",
                Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG"
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
            await StagingSession.AddMediaItems(Directory.GetFiles(selectedFolder, "*.*", SearchOption.AllDirectories));
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
            await StagingSession.SaveToRepository();
        }
        #endregion

        
    }
}
