using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaCenter.MVVM;
using MediaCenter.Repository;

namespace MediaCenter.Helpers
{
    public class StatusViewModel : PropertyChangedNotifier, IStatusService
    {
        private readonly IRepository _repository;

        public StatusViewModel(IRepository repository)
        {
            _repository = repository;
        }

        public void PostStatusMessage(string message)
        {

        }

        public void StartProgress() { }

        /// <summary>
        /// Updates the currently running progress indication
        /// </summary>
        /// <param name="progress">Value between 0 and 100</param>
        public void UpdateProgress(int progress)
        {

        }

        public void EndProgress() { }
    }
}
