using System.Collections.Generic;
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
                foreach (var item in editViewModel.Items)
                {
                    StagingSession.EditStagedItemDate(item, editViewModel.NewDateTaken);
                }
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
        
        #region Command: Remove staged items
        
        private RelayCommand<object> _removeItemsCommand;
        public RelayCommand<object> RemoveItemsCommand => _removeItemsCommand ?? (_removeItemsCommand = new RelayCommand<object>(RemoveItems));

        private void RemoveItems(object items)
        {
            var list = (System.Collections.IList) items;

            foreach (var item in list.Cast<StagedItem>().ToList())
            {
                StagingSession.RemoveStagedItem(item);
            }
            
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
