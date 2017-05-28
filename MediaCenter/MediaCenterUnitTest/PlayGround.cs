using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using MediaCenter.Helpers;
using MediaCenter.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MediaCenterUnitTest
{
    [TestClass]
    public class PlayGround
    {
        [TestMethod]
        public void TestMethod1()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<CatalogItem>));

            List<CatalogItem> collection = new List<CatalogItem>
            {
                new CatalogItem("Item 1")
                {
                    Tags = new List<string> {"Tag 1", "Tag 2"}
                },
                new CatalogItem("Item 2")
                {
                    Tags = new List<string> {"Tag 3", "Tag 4"}
                }
            };

            var jsonText = SerializationHelper.Serialize(collection);

        }
    }
}
