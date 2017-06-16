using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using MediaCenter.Helpers;
using MediaCenter.Media;
using MediaCenter.Repository;
using MediaCenter.Sessions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MediaCenterUnitTest
{
    [TestClass]
    public class PlayGround
    {
        [TestMethod]
        public void TestMethod1()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<MediaItem>));

            List<MediaItem> collection = new List<MediaItem>
            {
                new ImageItem("Item 1")
                {
                    DateTaken = DateTime.Now,
                    DateAdded = DateTime.Now,
            
                    Tags = new ObservableCollection<string> {"Tag 1", "Tag 2"}
                },
                new ImageItem("Item 2")
                {
                    DateTaken = DateTime.Now,
                    DateAdded = DateTime.Now,

                    Tags = new ObservableCollection<string> {"Tag 3", "Tag 4"}
                }
            };

            var jsonText = SerializationHelper.Serialize(collection.Select(i => new MediaInfo(i)));

        }

        [TestMethod]
        public void TestImageRotate()
        {
            var image = File.ReadAllBytes(@"c:\TEMP\Personal documents\MCTest\ImageTesting\DSC_0001.JPG");

            var rotatedIMageR = ImageHelper.Rotate(image, RotationDirection.Clockwise);
            var rotatedIMageL = ImageHelper.Rotate(image, RotationDirection.Counterclockwise);

            File.WriteAllBytes(@"c:\TEMP\Personal documents\MCTest\ImageTesting\DSC_0001_R.JPG", rotatedIMageR);
            File.WriteAllBytes(@"c:\TEMP\Personal documents\MCTest\ImageTesting\DSC_0001_L.JPG", rotatedIMageL);
        }

        [TestMethod]
        public void TestThumbnail()
        {
            var bitmap = File.ReadAllBytes(@"c:\TEMP\Personal documents\MCTest\TestImages\20160924202350.JPG");

            var thumbnail = ImageHelper.CreateThumbnail(bitmap,100, true);

            File.WriteAllBytes(@"c:\TEMP\Personal documents\MCTest\TestImages\20160924202350_T.JPG", thumbnail);

            thumbnail = ImageHelper.CreateThumbnail(bitmap, 100, false);

            File.WriteAllBytes(@"c:\TEMP\Personal documents\MCTest\TestImages\20160924202350_F.JPG", thumbnail);
        }
    }
}
