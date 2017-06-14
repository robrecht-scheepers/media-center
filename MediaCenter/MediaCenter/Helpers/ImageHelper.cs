using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Media;

namespace MediaCenter.Helpers
{
    public static class ImageHelper
    {
        public static async Task<byte[]> CreateThumbnail(Image source)
        {
            Image thumbnail = null;
            Image.GetThumbnailImageAbort myCallback = ThumbnailCallback;
            await Task.Run(() => { thumbnail = source.GetThumbnailImage(100, 100, myCallback, IntPtr.Zero); });

            MemoryStream ms = new MemoryStream();
            thumbnail.Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }
        public static bool ThumbnailCallback()
        {
            return false;
        }

        public static DateTime ReadImageDate(Image image)
        {
            int datePropertyID = 36867;
            ASCIIEncoding encoding = new ASCIIEncoding();
            string dateString = "";

            PropertyItem[] propertyItems = image.PropertyItems;
            var dateProperty = propertyItems.FirstOrDefault(p => p.Id == datePropertyID);
            if (dateProperty == null)
            {
                return DateTime.MinValue;
            }

            dateString = encoding.GetString(dateProperty.Value);
            dateString = dateString.Substring(0, dateString.Length - 1); // drop zero character /0
            var date = DateTime.ParseExact(dateString, "yyyy:MM:dd HH:mm:ss", new DateTimeFormatInfo());
            return date;
        }

        public static byte[] Rotate(byte[] image, RotationDirection direction)
        {
            using (var sourceStream = new MemoryStream(image))
            {
                var bitmap = Image.FromStream(sourceStream);

                RotateFlipType rotationtype = RotateFlipType.RotateNoneFlipNone;
                switch (direction)
                {
                    case RotationDirection.Clockwise:
                        rotationtype = RotateFlipType.Rotate90FlipNone;
                        break;
                    case RotationDirection.Counterclockwise:
                        rotationtype = RotateFlipType.Rotate270FlipNone;
                        break;
                }
                bitmap.RotateFlip(rotationtype);

                using (var destinationStream = new MemoryStream())
                {
                    bitmap.Save(destinationStream,ImageFormat.Jpeg);
                    return destinationStream.ToArray();
                }
            }
        }

        
    }
}
