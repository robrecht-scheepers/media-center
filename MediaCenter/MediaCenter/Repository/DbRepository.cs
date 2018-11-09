using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Helpers;
using MediaCenter.Media;
using MediaCenter.Sessions.Staging;

namespace MediaCenter.Repository
{
    public class DbRepository : IRepository
    {
        private readonly string _mediaFolderPath;
        private readonly string _thumbnailFolderPath;
        private readonly Database _database;


        public DbRepository(string dbPath, string mediaFolderPath, string thumbnailFolderPath)
        {
            _database = new Database(dbPath);
            _mediaFolderPath = mediaFolderPath;
            _thumbnailFolderPath = thumbnailFolderPath;
        }

        public event EventHandler CollectionChanged;
        public event EventHandler StatusChanged;
        public IEnumerable<MediaItem> Catalog { get; }
        public IEnumerable<string> Tags { get; }
        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        public async Task SaveNewItems(IEnumerable<StagedItem> newItems)
        {
            int saveCount = 0;
            var itemsToSave = newItems.Where(x => x.Status == MediaItemStatus.Staged).ToList();
            var count = itemsToSave.Count;
            var i = 1;

            foreach (var newItem in itemsToSave)
            {
                StatusMessage = $"Saving item {i++} of {count}";
                var originalName = newItem.Name;

                try
                {
                    if (string.IsNullOrEmpty(newItem.FilePath) || string.IsNullOrEmpty(newItem.Name))
                    {
                        newItem.Status = MediaItemStatus.Error;
                        continue;
                    }

                    newItem.Name = CreateUniqueName(originalName);
                    newItem.ContentFileName = newItem.Name + Path.GetExtension(newItem.FilePath);
                    var mediaItemFilePath = GetMediaPath(newItem);
                    await IOHelper.CopyFile(newItem.FilePath, mediaItemFilePath);
                    await IOHelper.SaveBytes(newItem.Thumbnail, GetThumbnailPath(newItem));
                    await _database.AddMediaInfo(newItem);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        private async Task AddMediaItemInfo(MediaItem newItem)
        {
            using()
        }

        private string CreateUniqueName(string originalName)
        {
            throw new NotImplementedException();
        }

        private string GetMediaPath(MediaItem item)
        {
            return Path.Combine(_mediaFolderPath, item.ContentFileName);
        }
        private string GetThumbnailPath(MediaItem item)
        {
            return Path.Combine(_thumbnailFolderPath, item.Name + "_T.jpg");
        }

        public Task DeleteItem(string name)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetThumbnail(string name)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetFullImage(string name, IEnumerable<string> prefetch)
        {
            throw new NotImplementedException();
        }

        public Task SaveItem(string name)
        {
            throw new NotImplementedException();
        }

        public Uri Location { get; }
        public Task SaveContentToFile(MediaItem item, string filePath)
        {
            throw new NotImplementedException();
        }

        public Task SaveContentToFolder(List<MediaItem> items, string folderPath)
        {
            throw new NotImplementedException();
        }

        public string StatusMessage { get; private set; }
    }
}
