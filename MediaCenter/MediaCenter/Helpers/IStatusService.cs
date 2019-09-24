using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.Helpers
{
    public interface IStatusService
    {
        /// <summary>
        /// Sends a message to be shown as status
        /// </summary>
        /// <param name="message">the message to be shown</param>
        /// <param name="keep">true if the message should not be automatically disappear after the status timeout. Default value is false</param>
        void PostStatusMessage(string message, bool keep = false);
        void ClearStatusMessage();

        void StartProgress();

        /// <summary>
        /// Updates the currently running progress indication
        /// </summary>
        /// <param name="progress">Value between 0 and 100</param>
        void UpdateProgress(int progress);

        void EndProgress();
    }
}
