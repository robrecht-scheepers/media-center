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
        public static async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                await sourceStream.CopyToAsync(destinationStream);
        }

        public static async Task SaveImageAsync(Image image, string filePath, ImageFormat format)
        {
            await Task.Run(() => image.Save(filePath, format));

            //using (MemoryStream ms = new MemoryStream())
            //using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            //{
            //    image.Save(ms,format);
            //    await ms.CopyToAsync(fs);
            //}
        }

        public static async Task SaveTextAsync(string content, string filePath)
        {
            using (StreamWriter sw = File.CreateText(filePath))
            {
                await sw.WriteAsync(content);
            }
        }
    }
}
