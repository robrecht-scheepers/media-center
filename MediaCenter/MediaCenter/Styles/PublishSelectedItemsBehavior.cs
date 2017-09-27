using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace MediaCenter.Styles
{
    public class PublishSelectedItemsBehavior : Behavior<ListBox>
    {
        public static readonly DependencyProperty SelectionsProperty =
            DependencyProperty.Register(
                "Selections",
                typeof(IList),
                typeof(PublishSelectedItemsBehavior),
                new PropertyMetadata(null));

        private bool _updating;
        
        public IList Selections
        {
            get { return (IList)this.GetValue(SelectionsProperty); }
            set { this.SetValue(SelectionsProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            Selections = new List<object>();
            this.AssociatedObject.SelectionChanged += this.OnSelectedItemsChanged;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.SelectionChanged -= this.OnSelectedItemsChanged;

            base.OnDetaching();
        }
        
        private void OnSelectedItemsChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelections(e);
        }

        private void UpdateSelections(SelectionChangedEventArgs e)
        {
            ExecuteIfNotUpdating(
                () =>
                {
                    if (this.Selections != null)
                    {
                        foreach (var item in e.AddedItems)
                        {
                            Selections.Add(item);
                        }

                        foreach (var item in e.RemovedItems)
                        {
                            Selections.Remove(item);
                        }
                    }
                });
        }
        
        private void ExecuteIfNotUpdating(Action execute)
        {
            if (!this._updating)
            {
                try
                {
                    this._updating = true;
                    execute();
                }
                finally
                {
                    this._updating = false;
                }
            }
        }

    }
}
