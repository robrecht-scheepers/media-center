using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoPlayerPOC
{
    public class Bootstrapper
    {
        public void Run()
        {
            var mainViewModel = new MainViewModel();
            var mainView = new MainWindow { DataContext = mainViewModel };
            mainView.Show();
        }

        public void Exit() { }
    }
}
