using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.MVVM
{
    public class SmartObservableCollection<T> : ObservableCollection<T>
    {
        public SmartObservableCollection() : base()
        {
        }

        public SmartObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public SmartObservableCollection(List<T> list) : base(list)
        {
        }

        public void ReplaceAllItems(IEnumerable<T> newItems)
        {
            var args = new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace,
                newItems.Except(Items).ToList(),
                Items.Except(newItems).ToList());

            Items.Clear();
            foreach (var newItem in newItems)
            {
                Items.Add(newItem);
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(args);
        }
    }
}
