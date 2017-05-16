using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;

namespace MediaCenter.Sessions
{
    public class StagedItem : Observable
    {
        private Image _thumbnail;
        public Image Thumbnail
        {
            get { return _thumbnail; }
            set { SetValue(ref _thumbnail, value);}
        }

        private string _name;
        public string Name 
        {
            get { return _name; }
            set { SetValue(ref _name, value); }
        }

        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set { SetValue(ref _filePath, value); }
        }



    }
}
