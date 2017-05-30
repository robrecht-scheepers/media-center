using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
            return await Task.Run(() => Image.FromFile(filePath));
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
    }
}
