//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using MediaCenter.Helpers;
//using MediaCenter.Media;
//using MediaCenter.Repository;

//namespace MediaCenter.Sessions.Staging
//{
//    public class StagingSession : SessionBase
//    {
//        // TODO: share with view model for dialog filter
//        private string _statusMessage;
//        private readonly Dictionary<string, string> _filePaths = new Dictionary<string, string>();
        

//        public StagingSession(IRepository repository) : base(repository)
//        {
           
//        }

        

        

        

        

//        public void EditStagedItemDate(StagedItem item, DateTime newDate)
//        {
//            if(item.DateTaken.Equals(newDate))
//                return;

//            var oldName = item.Name;
//            item.Name = CreateUniqueItemName(newDate);
//            item.DateTaken = newDate;
//        }
        

//        public async Task SaveToRepository(IEnumerable<string> tags)
//        {
//            foreach(var item in StagedItems)
//            {
//                item.Tags = new ObservableCollection<string>(tags.ToList());
//            }
//            await SaveToRepository();
//        }
        

        



        

//    }
//}
