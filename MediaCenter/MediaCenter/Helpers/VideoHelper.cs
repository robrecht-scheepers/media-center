using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Shell;

namespace MediaCenter.Helpers
{
    public static class VideoHelper
    {
        public static async Task<byte[]> CreateThumbnail(string filePath, int size)
        {
            // run ffmpeg.exe from command line in a separate process to
            // extract the first fram into a temp file.
            // Then read the temp file and create the thumbnail from it
            var firstFrameFile = @"C:\TEMP\thumbnail.jpg";

            var ffmpeg = new Process
            {
                StartInfo =
                {
                    Arguments = $" -i \"{filePath}\" \"{firstFrameFile}\"",
                    FileName = "Dependencies/ffmpeg.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            ffmpeg.Start();
            await ffmpeg.WaitForExitAsync();
            ffmpeg.Close();

            var firstFrame = await IOHelper.OpenBytes(firstFrameFile);
            File.Delete(firstFrameFile);
            return ImageHelper.CreateThumbnail(firstFrame, size);
        }

        public static DateTime ReadCreationDate(string filePath)
        {
            var file = ShellFile.FromFilePath(filePath);
            return file.Properties.System.DateModified.Value ?? DateTime.MinValue;
        }
    }
}
