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
            await Task.Run(() => File.Copy(sourceFile, destinationFile, true));
        }

        public static async Task MoveFile(string sourceFile, string destinationFile)
        {
            await Task.Run(() => File.Move(sourceFile, destinationFile));
        }

        public static async Task SaveImage(Image image, string filePath, ImageFormat format)
        {
            await Task.Run(() => image.Save(filePath, format));
        }

        public static async Task<Image> OpenImage(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            return await Task.Run(() =>
            {
                try
                {
                    var bytes = File.ReadAllBytes(filePath);
                    var ms = new MemoryStream(bytes);
                    return new Bitmap(ms);
                }
                catch (Exception)
                {
                    return null;
                }
            });
        }

        public static async Task<byte[]> OpenBytes(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

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
            if (!File.Exists(filePath))
                return null;

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
            if (!File.Exists(filePath))
                return default(T);

            string serialized = await OpenText(filePath);
            T result = SerializationHelper.Deserialize<T>(serialized);
            return result;
        }

        public static async Task DeleteFile(string filePath)
        {
            await Task.Run(() => File.Delete(filePath));
        }

    }
}
