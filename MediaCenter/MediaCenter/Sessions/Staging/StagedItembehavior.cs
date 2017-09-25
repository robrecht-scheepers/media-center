using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace MediaCenter.Sessions.Staging
{
    public class StagedItembehavior : Behavior<StagedItemView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObjectOnMouseLeave;

            var view = AssociatedObject as StagedItemView;
            if (view == null || view.ButtonList == null)
                return;
            view.ButtonList.Visibility = Visibility.Collapsed;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave -= AssociatedObjectOnMouseLeave;
        }

        private void AssociatedObject_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Debug.WriteLine("Enter");
            var view = sender as StagedItemView;
            if (view == null || view.ButtonList == null)
                return;
            view.ButtonList.Visibility = Visibility.Visible;
        }
        private void AssociatedObjectOnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            Debug.WriteLine("Leave");
            var view = sender as StagedItemView;
            if (view == null || view.ButtonList == null)
                return;
            view.ButtonList.Visibility = Visibility.Collapsed;
        }

        
    }
}
