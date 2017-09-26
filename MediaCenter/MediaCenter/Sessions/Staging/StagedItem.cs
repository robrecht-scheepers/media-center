using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.Media;

namespace MediaCenter.Sessions.Staging
{
    public class StagedItem : MediaItem
    {
        private string _filePath;

        public StagedItem(string name, MediaType type) : base(name, type)
        { }

        public string FilePath
        {
            get { return _filePath; }
            set { SetValue(ref _filePath, value); }
        }
    }
}
