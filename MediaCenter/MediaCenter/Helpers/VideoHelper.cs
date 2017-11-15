using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Shell;

namespace MediaCenter.Helpers
{
    public static class VideoHelper
    {
        public static async Task<byte[]> CreateThumbnail(string filePath, int size)
        {
            var firstFrame = await GetFirstFrameImage(filePath);
            return ImageHelper.CreateThumbnail(firstFrame, size);
        }

        public static async Task<byte[]> GetFirstFrameImage(string filePath)
        {
            // run ffmpeg.exe from command line in a separate process to
            // extract the first frame into a temp file. Then read the temp 
            // file and delete it afterwards
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
            var error = ffmpeg.StandardError.ReadToEnd();
            ffmpeg.Close();

            var result = await IOHelper.OpenBytes(firstFrameFile);
            await IOHelper.DeleteFile(firstFrameFile);
            return result;
        }

        public static DateTime ReadCreationDate(string filePath)
        {
            var file = ShellFile.FromFilePath(filePath);
            return file.Properties.System.DateModified.Value ?? DateTime.MinValue;
        }

        public static int ReadRotation(string filePath)
        {
            // run ffmpeg.exe from command line in a separate process 
            // and read the rotation output from the StandardOutput
            var ffprobe = new Process
            {
                StartInfo =
                {
                    FileName = "Dependencies/ffprobe.exe",
                    Arguments =
                        $"-v error -show_entries stream_tags=rotate -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            
            if (!ffprobe.Start())
            {
                return 0;
            }
            string output = ffprobe.StandardOutput.ReadToEnd().Replace("\r\n","");
            string error = ffprobe.StandardError.ReadToEnd();
            ffprobe.WaitForExit();
            ffprobe.Close();

            int result = 0;
            if (!string.IsNullOrEmpty(output.ToString()))
            {
                int.TryParse(output.ToString(), out result);
            }
            return result;
        }
    }
}
