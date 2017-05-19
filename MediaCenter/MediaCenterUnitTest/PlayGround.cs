using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using MediaCenter.Helpers;
using MediaCenter.MediaItems;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MediaCenterUnitTest
{
    [TestClass]
    public class PlayGround
    {
        [TestMethod]
        public void TestMethod1()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(MediaItemCollection));

            MediaItemCollection collection = new MediaItemCollection
            {
                Items = new List<MediaItem>
                { 
                    new ImageItem("Item 1")
                    {
                        Tags = new List<string> {"Tag 1", "Tag 2"}
                    },
                    new ImageItem("Item 2")
                    {
                        Tags = new List<string> {"Tag 3", "Tag 4"}
                    }
                }
            };

            var jsonText = SerializationHelper.Serialize(collection);

        }
    }
}
