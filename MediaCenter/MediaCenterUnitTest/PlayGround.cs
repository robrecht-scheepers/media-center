using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using MediaCenter.Helpers;
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
                new MediaItem("Item 1", MediaType.Image)
                {
                    DateTaken = DateTime.Now,
                    DateAdded = DateTime.Now,
            
                    Tags = new ObservableCollection<string> {"Tag 1", "Tag 2"}
                },
                new MediaItem("Item 2", MediaType.Image)
                {
                    DateTaken = DateTime.Now,
                    DateAdded = DateTime.Now,

                    Tags = new ObservableCollection<string> {"Tag 3", "Tag 4"}
                }
            };

            var jsonText = SerializationHelper.Serialize(collection);

        }
    }
}
