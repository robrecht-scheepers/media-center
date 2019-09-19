using System;
using System.Drawing;
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

        public static DateTime ReadCreationDate(Image image)
        {
            int datePropertyID = 36867;
            ASCIIEncoding encoding = new ASCIIEncoding();
            DateTime creationDate = DateTime.MinValue.AddDays(1);

            PropertyItem[] propertyItems = image.PropertyItems;
            var dateProperty = propertyItems.FirstOrDefault(p => p.Id == datePropertyID);
            if (dateProperty == null)
            {
                return creationDate;
            }

            var dateString = encoding.GetString(dateProperty.Value);
            dateString = dateString.Substring(0, dateString.Length - 1); // drop last zero character /0
            DateTime.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss", new DateTimeFormatInfo(), DateTimeStyles.None, out creationDate);
            return creationDate;
        }

        public static int ReadRotation(Image image)
        {
            int rotationPropertyID = 0x112; //274

            ASCIIEncoding encoding = new ASCIIEncoding();
            PropertyItem[] propertyItems = image.PropertyItems;
            var rotationProperty = propertyItems.FirstOrDefault(p => p.Id == rotationPropertyID);
            if (rotationProperty == null)
            {
                return 0;
            }

            int val = BitConverter.ToUInt16(rotationProperty.Value, 0);
            
            if (val == 3 || val == 4)
                return 180;
            if (val == 5 || val == 6)
                return 90;
            if (val == 7 || val == 8)
                return 270;

            return 0;
        }

        public static byte[] CropImage(byte[] image, Crop crop)
        {
            using (var sourceStream = new MemoryStream(image))
            {
                var sourceImage = Image.FromStream(sourceStream);

                var cropRectangle = new Rectangle(
                    (int)(crop.X * sourceImage.Width),
                    (int)(crop.Y * sourceImage.Height),
                    (int)(crop.Width * sourceImage.Width),
                    (int)(crop.Height * sourceImage.Height));

                var croppedImage = new Bitmap(cropRectangle.Width, cropRectangle.Height);

                using (var g = Graphics.FromImage(croppedImage))
                {
                    g.DrawImage(sourceImage, new Rectangle(0,0,croppedImage.Width, croppedImage.Height), 
                        cropRectangle, GraphicsUnit.Pixel);
                }

                using (var destinationStream = new MemoryStream())
                {
                    croppedImage.Save(destinationStream, ImageFormat.Jpeg);
                    return destinationStream.ToArray();
                }
            }
        }

        /// <summary>
        /// returns a rotated version of the original image 
        /// </summary>
        /// <param name="image">The image to be rotated</param>
        /// <param name="angle">The angle. Allowed values are: 0, 90, 180, 270. Other values will cause an argument exception</param>
        /// <returns></returns>
        public static byte[] Rotate(byte[] image, int angle)
        {
            RotateFlipType rotationType;
            switch (angle)
            {
                case 0:
                    rotationType = RotateFlipType.RotateNoneFlipNone;
                    break;
                case 90:
                    rotationType = RotateFlipType.Rotate90FlipNone;
                    break;
                case 180:
                    rotationType = RotateFlipType.Rotate180FlipNone;
                    break;
                case 270:
                    rotationType = RotateFlipType.Rotate270FlipNone;
                    break;
                default:
                    throw new ArgumentException($"{angle} is not a valid image rotation angle. Only 0, 90 180 and 270 are valid rotation angles.");
            }

            using (var sourceStream = new MemoryStream(image))
            {
                var bitmap = Image.FromStream(sourceStream);

                bitmap.RotateFlip(rotationType);

                using (var destinationStream = new MemoryStream())
                {
                    bitmap.Save(destinationStream, ImageFormat.Jpeg);
                    return destinationStream.ToArray();
                }
            }
        }
        
    }
}
