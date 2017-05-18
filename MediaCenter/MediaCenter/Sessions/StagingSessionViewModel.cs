using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaCenter.MVVM;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace MediaCenter.Sessions
{
    public class StagingSessionViewModel : SessionViewModelBase
    {
        public StagingSessionViewModel(StagingSession session) : base(session)
        {
            StagedItems = new ObservableCollection<StagedItemViewModel>(session.StagedItems.Select(x => new StagedItemViewModel(x)));
            session.StagedItems.CollectionChanged += StagedItems_CollectionChanged;
        }

        private void StagedItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    StagedItems.Remove(StagedItems.First(x => x.FilePath == ((StagedItem) oldItem).FilePath));
                }
            }
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    StagedItems.Add(new StagedItemViewModel((StagedItem) newItem));
                }
            }
        }

        public StagingSession StagingSession => (StagingSession)Session;

        public ObservableCollection<StagedItemViewModel> StagedItems { get; } 
        
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
            if(!selectedImages.Any())
                return;
            await StagingSession.AddMediaItems(selectedImages);
        }

        private AsyncRelayCommand _addDirectoryCommand;
        public AsyncRelayCommand AddDirectoryCommand => _addDirectoryCommand ?? (_addDirectoryCommand = new AsyncRelayCommand(AddDirectory));
        private async Task AddDirectory()
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();

            var selectedFolder = dialog.SelectedPath;
            if (string.IsNullOrEmpty(selectedFolder))
                return;
            await StagingSession.AddMediaItems(Directory.GetFiles(selectedFolder));
        }

        private AsyncRelayCommand _saveToRepositoryCommand;
        public AsyncRelayCommand SaveToRepositoryCommand => _saveToRepositoryCommand ?? (_saveToRepositoryCommand = new AsyncRelayCommand(SaveToRepository,CanExecuteSaveToRepository));

        private bool CanExecuteSaveToRepository()
        {
            return StagedItems.Any();
        }

        private async Task SaveToRepository()
        {
            await StagingSession.SaveToRepository();
        }

        protected override string CreateNameForSession(SessionBase session)
        {
            return "Staging 1";
        }
    }
}
