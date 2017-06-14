using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
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
        private readonly string[] _imageExtensions = {".jpg", ".png", ".bmp"};
        private string _statusMessage;
        private readonly Dictionary<string, string> _filePaths = new Dictionary<string, string>(); 

        public StagingSession(RemoteRepository repository) : base(repository)
        {
            StagedItems = new ObservableCollection<MediaItem>();
        }

        public ObservableCollection<MediaItem> StagedItems { get; }

        public async Task AddMediaItems(IEnumerable<string> newItems)
        {
            var newItemsList = newItems.ToList();
            var total = newItemsList.Count();
            var cnt = 1;

            foreach (var filePath in newItemsList)
            {
                StatusMessage = $"Loading item {cnt++} of {total}.";
                if ((string.IsNullOrEmpty(filePath))||(!_imageExtensions.Contains(Path.GetExtension(filePath).ToLower())))
                    continue;

                Image image = null;
                try
                {
                    image = await IOHelper.OpenImage(filePath);
                    if (image == null)
                    {
                        // TODO: create error list
                        continue;
                    }

                    var date = ImageHelper.ReadImageDate(image);
                    var name = CreateItemName(date);
                    var thumbnail = await ImageHelper.CreateThumbnail(image);
                    image.Dispose();

                    StagedItems.Add(new MediaItem(name, MediaType.Image)
                    {
                        DateTaken = date,
                        DateAdded = DateTime.Now,
                        Thumbnail = thumbnail
                    });
                    _filePaths[name] = filePath;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    // TODO: create error list
                }
                finally
                {
                    image?.Dispose();
                }
            }
        }

        public void RemoveStagedItem(MediaItem item)
        {
            if (StagedItems.Contains(item))
            {
                StagedItems.Remove(item);
            }
            if (_filePaths.ContainsKey(item.Name))
                _filePaths.Remove(item.Name);
        }

        public async Task SaveToRepository()
        {
            StatusMessage = $"Saving {StagedItems.Count} items...";
            await Repository.SaveStagedItems(StagedItems.Select(s => new KeyValuePair<string, MediaItem>(_filePaths[s.Name], s)));
            StatusMessage = "All items saved";
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetValue(ref _statusMessage, value); }
        }

        private string CreateItemName(DateTime date)
        {
            var name = date.ToString("yyyyMMddHHmmss");
            //TODO: guarantee name uniqueness
            return name;
        }

    }
}
