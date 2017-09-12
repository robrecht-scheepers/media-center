using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media.Animation;
using Shell32;

namespace MediaCenter.Helpers
{
    public static class VideoHelper
    {
        public static byte[] CreateThumbnail(string filePath, int size)
        {
            throw new NotImplementedException();
        }

        public static DateTime ReadCreationDate(string filePath)
        {
            Dictionary<int,string> headers = new Dictionary<int, string>();
            Dictionary<string, string> values = new Dictionary<string, string>();

            var shell = new Shell32.Shell();
            var folder = shell.NameSpace(Path.GetDirectoryName(filePath));
            var file = folder.Items().Cast<FolderItem2>().FirstOrDefault(folderItem => folderItem.Path == filePath);

            for (int i = 0; i < short.MaxValue; i++)
            {
                var header = folder.GetDetailsOf(null, i);
                if (string.IsNullOrEmpty(header))
                    break;
                headers[i] = header;
            }

            for (int i = 0; i < short.MaxValue; i++)
            {
                var value = folder.GetDetailsOf(file, i);
                if (string.IsNullOrEmpty(value))
                    break;
                values[headers[i]] = value;
            }

            var dateString = folder.GetDetailsOf(file, 3);
            return DateTime.Parse(dateString);
        }
    }
}
