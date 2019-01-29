using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace MediaCenter.Helpers
{
    public static class IOHelper
    {
        public static async Task CopyFile(string sourceFile, string destinationFile)
        {
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                await sourceStream.CopyToAsync(destinationStream);
        }

        public static async Task SaveImage(Image image, string filePath, ImageFormat format)
        {
            await Task.Run(() => image.Save(filePath, format));
        }

        public static async Task<Image> OpenImage(string filePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var bytes = File.ReadAllBytes(filePath);
                    var ms = new MemoryStream(bytes);
                    return new Bitmap(ms);
                }
                catch (Exception e)
                {
                    // todo: better error handling
                    return null;
                }
            });
        }

        public static async Task<byte[]> OpenBytes(string filePath)
        {
            return await Task.Run(() => File.ReadAllBytes(filePath));
        }

        public static async Task SaveBytes(byte[] content, string filePath)
        {
            await Task.Run(() => File.WriteAllBytes(filePath, content));
        }

        public static async Task SaveText(string content, string filePath)
        {
            using (StreamWriter sw = File.CreateText(filePath))
            {
                await sw.WriteAsync(content);
            }
        }

        public static async Task<string> OpenText(string filePath)
        {
            string content = null;
            using (StreamReader reader = File.OpenText(filePath))
            {
                content = await reader.ReadToEndAsync();
            }
            return content;
        }

        public static async Task SaveObject<T>(T t, string filePath)
        {
            var serializedObject = SerializationHelper.Serialize(t);
            await SaveText(serializedObject, filePath);
        }

        public static async Task<T> OpenObject<T>(string filePath)
        {
            string serialized = await OpenText(filePath);
            T result = SerializationHelper.Deserialize<T>(serialized);
            return result;
        }

        public static async Task DeleteFile(string filePath)
        {
            await Task.Run(() => File.Delete(filePath));
        }

        public static async Task<FileInfo[]> GetFiles(string directoryPath, string searchPattern)
        {
            return await Task.Run(() => new DirectoryInfo(directoryPath).GetFiles(searchPattern));
        }

    }
}
