using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.Helpers
{
    public interface IStatusService
    {
        void PostStatusMessage(string message);
        void StartProgress();

        /// <summary>
        /// Updates the currently running progress indication
        /// </summary>
        /// <param name="progress">Value between 0 and 100</param>
        void UpdateProgress(int progress);

        void EndProgress();
    }
}
