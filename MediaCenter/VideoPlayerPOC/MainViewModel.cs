using MediaCenter.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoPlayerPOC
{
    public class MainViewModel : PropertyChangedNotifier 
    {
        public VideoPLayerViewModel VideoPLayerViewModel { get; }

        public MainViewModel()
        {
            VideoPLayerViewModel = new VideoPLayerViewModel(@"c:\Users\scheepers\Desktop\test\MOV_0967.mp4");
        }
    }
}
