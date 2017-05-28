using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MediaCenter.Repository
{
    public class CatalogItem
    {
        public CatalogItem(string name)
        {
            Name = name;
            Tags = new List<string>();
        }

        public string Name { get; set; }

        public List<string> Tags { get; set; }

        public DateTime DateTaken { get; set; }

        public bool Favorite { get; set; }

        public void UpdateFrom(CatalogItem item)
        {
            Name = item.Name;
            DateTaken = item.DateTaken;
            Favorite = item.Favorite;
            Tags.Clear();
            Tags.InsertRange(0,item.Tags);
        }
    }
}
