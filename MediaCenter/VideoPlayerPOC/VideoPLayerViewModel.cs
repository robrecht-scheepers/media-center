using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;

namespace VideoPlayerPOC
{
    public class VideoPLayerViewModel : PropertyChangedNotifier
    {
        private string _filePath;
        public VideoPLayerViewModel(string filePath)
        {
            _filePath = filePath;
        }


    }
}
