using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaCenter.Helpers;
using MediaCenter.Media;
using MediaCenter.Repository;

namespace MediaCenter.Sessions.Staging
{
    public class StagingSession : SessionBase
    {
        // TODO: share with view model for dialog filter
        private readonly string[] _supportedImageExtensions = {".jpg", ".jpeg", ".png", ".bmp"};
        private readonly string[] _supportedVideoExtensions = { ".mp4", ".avi", ".mts"};
        private string _statusMessage;
        private readonly Dictionary<string, string> _filePaths = new Dictionary<string, string>();
        

        public StagingSession(IRepository repository) : base(repository)
        {
            StagedItems = new ObservableCollection<StagedItem>();
        }

        public ObservableCollection<StagedItem> StagedItems { get; }

        

        public async Task AddMediaItems(IEnumerable<string> newItems)
        {
            var newItemsList = newItems.ToList();
            var total = newItemsList.Count();
            var cnt = 1;

            foreach (var filePath in newItemsList)
            {
                StatusMessage = $"Loading item {cnt++} of {total}.";
                if (string.IsNullOrEmpty(filePath))
                    continue;
                var extension = Path.GetExtension(filePath).ToLower();
                if (!_supportedImageExtensions.Contains(extension) && !_supportedVideoExtensions.Contains(extension))
                    continue;

                try
                {
                    if (_supportedImageExtensions.Contains(extension))
                    {
                        using (var image = await IOHelper.OpenImage(filePath))
                        {
                            if (image == null)
                            {
                                // TODO: error handling, create list of failed files
                                Debug.WriteLine($"failed to load image {filePath}");
                                continue;
                            }

                            var dateTaken = ImageHelper.ReadCreationDate(image);
                            var name = CreateUniqueItemName(dateTaken);
                            var thumbnail = ImageHelper.CreateThumbnail(image, 100);

                            StagedItems.Add(new StagedItem(name, MediaType.Image)
                            {
                                FilePath = filePath,
                                Status = MediaItemStatus.Staged,
                                DateTaken = dateTaken,
                                DateAdded = DateTime.Now,
                                Thumbnail = thumbnail
                            });
                        }
                    }
                    else if(_supportedVideoExtensions.Contains(extension))
                    {
                        var dateTaken = VideoHelper.ReadCreationDate(filePath);
                        var name = CreateUniqueItemName(dateTaken);
                        var thumbnail = await VideoHelper.CreateThumbnail(filePath, 100);

                        StagedItems.Add(new StagedItem(name, MediaType.Video)
                        {
                            FilePath = filePath,
                            Status = MediaItemStatus.Staged,
                            DateTaken = dateTaken,
                            DateAdded = DateTime.Now,
                            Thumbnail = thumbnail
                        });
                        _filePaths[name] = filePath;                        
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    // TODO: error handling, create list of failed files
                }
            }
        }

        public void RemoveStagedItem(StagedItem item)
        {
            if (StagedItems.Contains(item))
            {
                StagedItems.Remove(item);
            }
            if (_filePaths.ContainsKey(item.Name))
                _filePaths.Remove(item.Name);
        }

        public void EditStagedItemDate(StagedItem item, DateTime newDate)
        {
            if(item.DateTaken.Equals(newDate))
                return;

            var oldName = item.Name;
            item.Name = CreateUniqueItemName(newDate);
            item.DateTaken = newDate;
        }
        

        public async Task SaveToRepository(IEnumerable<string> tags)
        {
            foreach(var item in StagedItems)
            {
                item.Tags = new ObservableCollection<string>(tags.ToList());
            }
            await SaveToRepository();
        }
        public async Task SaveToRepository()
        {
            StatusMessage = $"Saving {StagedItems.Count} items...";
            await Repository.SaveNewItems(StagedItems);
            StatusMessage = "All items saved";
            ClearSavedItems();
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetValue(ref _statusMessage, value); }
        }

        private string CreateUniqueItemName(DateTime date)
        {
            var baseName = date.ToString("yyyyMMddHHmmss");

            var name = baseName;
            var i = 1;
            while (StagedItems.Any(x => x.Name == name))
            {
                name = $"{baseName}_{i++}";
            }

            return name;
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
