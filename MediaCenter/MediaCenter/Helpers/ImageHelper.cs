using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MediaCenter.Media;

namespace MediaCenter.Helpers
{
    public static class ImageHelper
    {
        public static byte[] CreateThumbnail(byte[] image, int size, bool useEmbeddedWhenAvailable)
        {
            using (var imageStream = new MemoryStream(image))
            {
                var bitmap = Image.FromStream(imageStream);
                return CreateThumbnail(bitmap, size, useEmbeddedWhenAvailable);
            }
        }
        public static byte[] CreateThumbnail(Image image, int size, bool useEmbeddedWhenAvailable)
        {
            Image thumbnail;
            if (useEmbeddedWhenAvailable)
            {
                Image.GetThumbnailImageAbort myCallback = ThumbnailCallback;
                thumbnail = image.GetThumbnailImage(size, size, myCallback, IntPtr.Zero);
            }
            else
            {
                float scaleFactor = Math.Min((float)size / (float)image.Width, (float)size / (float)image.Height);
                var scaledWidth = (int)(image.Width * scaleFactor);
                var scaledHeight = (int)(image.Height * scaleFactor);
                thumbnail = new Bitmap(size, size);
                using (var graph = Graphics.FromImage(thumbnail))
                {
                    graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graph.CompositingQuality = CompositingQuality.HighQuality;
                    graph.SmoothingMode = SmoothingMode.AntiAlias;
                    graph.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    graph.FillRectangle(new SolidBrush(Color.Black), new RectangleF(0, 0, size, size));
                    graph.DrawImage(image,
                        new Rectangle((size - scaledWidth) / 2, (size - scaledHeight) / 2, scaledWidth,
                            scaledHeight));
                }
            }
            using (var resultStream = new MemoryStream())
            {
                thumbnail.Save(resultStream, ImageFormat.Png);
                return resultStream.ToArray();
            }
        }
        public static bool ThumbnailCallback()
        {
            return false;
        }

        public static DateTime ReadImageDate(byte[] image)
        {
            using (var imageStream = new MemoryStream(image))
            {
                var bitmap = Image.FromStream(imageStream);
                return ReadImageDate(bitmap);
            }
        }

        public static DateTime ReadImageDate(Image image)
        {
            int datePropertyID = 36867;
            ASCIIEncoding encoding = new ASCIIEncoding();

            PropertyItem[] propertyItems = image.PropertyItems;
            var dateProperty = propertyItems.FirstOrDefault(p => p.Id == datePropertyID);
            if (dateProperty == null)
            {
                return DateTime.MinValue;
            }

            var dateString = encoding.GetString(dateProperty.Value);
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
