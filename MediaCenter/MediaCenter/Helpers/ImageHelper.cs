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
        public static byte[] CreateThumbnail(byte[] image, int size)
        {
            using (var imageStream = new MemoryStream(image))
            {
                var bitmap = Image.FromStream(imageStream);
                return CreateThumbnail(bitmap, size);
            }
        }
        public static byte[] CreateThumbnail(Image image, int size)
        {
            float scaleFactor = Math.Max((float)size / (float)image.Width, (float)size / (float)image.Height);
            var scaledWidth = (int)(image.Width * scaleFactor);
            var scaledHeight = (int)(image.Height * scaleFactor);
            Image thumbnail = new Bitmap(size, size);
            using (var graph = Graphics.FromImage(thumbnail))
            {
                var scaledImage = new Bitmap(image, new Size(scaledWidth, scaledHeight));
                var cropRect = new Rectangle((scaledWidth-size)/2,(scaledHeight-size)/2,size,size);

                graph.DrawImage(scaledImage,new Rectangle(0,0,size,size),cropRect,GraphicsUnit.Pixel);
            }
            
            using (var resultStream = new MemoryStream())
            {
                thumbnail.Save(resultStream, ImageFormat.Png);
                return resultStream.ToArray();
            }
        }        

        public static DateTime ReadCreationDate(byte[] image)
        {
            using (var imageStream = new MemoryStream(image))
            {
                var bitmap = Image.FromStream(imageStream);
                return ReadCreationDate(bitmap);
            }
        }

        public static DateTime ReadCreationDate(Image image)
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
            dateString = dateString.Substring(0, dateString.Length - 1); // drop last zero character /0
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
