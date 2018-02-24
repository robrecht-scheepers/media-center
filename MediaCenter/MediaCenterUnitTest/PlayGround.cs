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
            

        }

        [TestMethod]
        public void TestImageRotate()
        {
            var image = File.ReadAllBytes(@"c:\TEMP\Personal documents\MCTest\ImageTesting\DSC_0001.JPG");

            var rotatedIMageR = ImageHelper.Rotate(image, 90);
            var rotatedIMageL = ImageHelper.Rotate(image, 270);

            File.WriteAllBytes(@"c:\TEMP\Personal documents\MCTest\ImageTesting\DSC_0001_R.JPG", rotatedIMageR);
            File.WriteAllBytes(@"c:\TEMP\Personal documents\MCTest\ImageTesting\DSC_0001_L.JPG", rotatedIMageL);
        }

        [TestMethod]
        public void TestThumbnail()
        {
            var bitmap = File.ReadAllBytes(@"c:\TEMP\Personal documents\MCTest\TestImages\20160924202350.JPG");

            var thumbnail = ImageHelper.CreateThumbnail(bitmap,100);

            File.WriteAllBytes(@"c:\TEMP\Personal documents\MCTest\TestImages\20160924202350_T.JPG", thumbnail);

            thumbnail = ImageHelper.CreateThumbnail(bitmap, 100);

            File.WriteAllBytes(@"c:\TEMP\Personal documents\MCTest\TestImages\20160924202350_F.JPG", thumbnail);
        }
    }
}
