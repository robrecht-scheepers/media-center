using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaCenter.Media;
using MediaCenter.MVVM;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using System.Configuration;
using System.Text;
using MediaCenter.Helpers;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Staging
{
    public class StagingSessionViewModel : SessionViewModelBase
    {
        private readonly string[] _supportedImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp" };
        private readonly string[] _supportedVideoExtensions = { ".mp4", ".avi", ".mts", ".m4v" };


        private EditMediaInfoViewModel _editMediaInfoViewModel;
        private AsyncRelayCommand _saveToRepositoryCommand;
        private RelayCommand<StagedItem> _showPreviewCommand;
        private AsyncRelayCommand _addDirectoryCommand;
        private StagedItem _previewItem;
        
        public StagingSessionViewModel(IRepository repository, IWindowService windowService, ShortcutService shortcutService, IStatusService statusService) 
            : base(repository, windowService, shortcutService, statusService)
        {
            StagedItems = new ObservableCollection<StagedItem>();
            SelectedItems = new BatchObservableCollection<MediaItem>();
            SelectedItems.CollectionChanged += async (s,a) => await SelectedItemsOnCollectionChanged(s,a);
            EditMediaInfoViewModel = new EditMediaInfoViewModel(Repository, ShortcutService, StatusService, false);
        }

        public override string Name => "Add media";
        public override Task Close()
        {
            return Task.CompletedTask;
        }

        public ObservableCollection<StagedItem> StagedItems { get; }

        

        public BatchObservableCollection<MediaItem> SelectedItems { get; }
        private async Task SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            EditMediaInfoViewModel.LoadItems(SelectedItems.ToList());
        }

        public EditMediaInfoViewModel EditMediaInfoViewModel
        {
            get => _editMediaInfoViewModel;
            set => SetValue(ref _editMediaInfoViewModel, value);
        }

        public StagedItem PreviewItem
        {
            get => _previewItem;
            set => SetValue(ref _previewItem, value);
        }

        public RelayCommand<StagedItem> ShowPreviewCommand => _showPreviewCommand ?? (_showPreviewCommand = new RelayCommand<StagedItem>(ShowPreview));
        private void ShowPreview(StagedItem item)
        {
            PreviewItem = item;
        }
        

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
            await AddMediaItems(selectedImages);
        }
        
        
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

            await AddMediaItems(Directory.GetFiles(selectedFolder, "*.*", searchOption));
        }
        
        
        private RelayCommand _removeItemsCommand;
        public RelayCommand RemoveItemsCommand => _removeItemsCommand ?? (_removeItemsCommand = new RelayCommand(RemoveItems));

        private void RemoveItems()
        {
            foreach (var item in SelectedItems.Cast<StagedItem>().ToList())
            {
                StagedItems.Remove(item);
            }
        }
        
        public AsyncRelayCommand SaveToRepositoryCommand => _saveToRepositoryCommand ?? (_saveToRepositoryCommand = new AsyncRelayCommand(SaveToRepository,CanExecuteSaveToRepository));
        private bool CanExecuteSaveToRepository()
        {
            return StagedItems.Any();
        }
        public async Task SaveToRepository()
        {
            // retry error items
            foreach (var stagedItem in StagedItems.Where(x => x.Status == MediaItemStatus.Error))
            {
                stagedItem.Status = MediaItemStatus.Staged;
            }

            var stagedCount = StagedItems.Count(x => x.Status == MediaItemStatus.Staged);
            var cnt = 1;
            StatusService.StartProgress();
            StatusService.PostStatusMessage($"Saving {stagedCount} items...", true);
            foreach (var stagedItem in StagedItems.Where(x => x.Status == MediaItemStatus.Staged))
            {
                await Repository.SaveNewItem(stagedItem);
                StatusService.UpdateProgress(cnt++*100/stagedCount);
            }
            StatusService.EndProgress();
            StatusService.PostStatusMessage($"Saved {StagedItems.Count(x => x.Status == MediaItemStatus.Saved)} items...");
            ClearSavedItems();
        }


        public async Task AddMediaItems(IEnumerable<string> newItems)
        {
            var newItemsList = newItems.ToList();
            var total = newItemsList.Count();
            var cnt = 1;
            var loaded = 0;

            var errors = new StringBuilder();

            StatusService.StartProgress();
            StatusService.PostStatusMessage($"loading {total} media files", true);
            foreach (var filePath in newItemsList)
            {
                StatusService.UpdateProgress((cnt++*100)/total);

                if (string.IsNullOrEmpty(filePath))
                    continue;
                var extension = Path.GetExtension(filePath).ToLower();
                
                try
                {
                    StagedItem newStagedItem = null;
                    if (_supportedImageExtensions.Contains(extension))
                    {
                        using (var image = await IOHelper.OpenImage(filePath))
                        {
                            if (image == null)
                            {
                                errors.AppendLine($"Error with file { filePath}: failed to load image.");
                                continue;
                            }

                            var dateTaken = ImageHelper.ReadCreationDate(image);
                            var thumbnail = ImageHelper.CreateThumbnail(image, 100);
                            var rotation = ImageHelper.ReadRotation(image);
                            
                            StagedItems.Add(newStagedItem = new StagedItem(MediaType.Image)
                            {
                                FilePath = filePath,
                                Status = MediaItemStatus.Staged,
                                DateTaken = dateTaken,
                                DateAdded = DateTime.Now,
                                Thumbnail = thumbnail,
                                Rotation = rotation
                            });
                        }
                    }
                    else if (_supportedVideoExtensions.Contains(extension))
                    {
                        var dateTaken = VideoHelper.ReadCreationDate(filePath);
                        var thumbnail = await VideoHelper.CreateThumbnail(filePath, 100);
                        var rotation = VideoHelper.ReadRotation(filePath);

                        StagedItems.Add(newStagedItem = new StagedItem(MediaType.Video)
                        {
                            FilePath = filePath,
                            Status = MediaItemStatus.Staged,
                            DateTaken = dateTaken,
                            DateAdded = DateTime.Now,
                            Thumbnail = thumbnail,
                            Rotation = rotation
                        });
                    }

                    if (newStagedItem != null && await Repository.IsDuplicate(newStagedItem))
                        newStagedItem.Status = MediaItemStatus.StagedDuplicate;
                    loaded++;
                }
                catch (Exception e)
                {
                    errors.AppendLine($"Error with file {filePath}: {e.Message}");
                }
            }
            StatusService.EndProgress();
            StatusService.PostStatusMessage($"loaded {loaded} media files");

            if (errors.Length > 0)
                WindowService.ShowMessage(errors.ToString(), "Fehler");
        }

        private void ClearSavedItems()
        {
            var savedList = StagedItems.Where(x => x.Status == MediaItemStatus.Saved).ToList();
            foreach (var mediaItem in savedList)
            {
                StagedItems.Remove(mediaItem);
            }
        }
    }
}
