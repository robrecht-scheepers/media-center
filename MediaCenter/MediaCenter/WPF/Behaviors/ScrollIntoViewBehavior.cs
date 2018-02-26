using System.Windows.Controls;
using System.Windows.Interactivity;

namespace MediaCenter.WPF.Behaviors
{
    public sealed class ScrollIntoViewBehavior:Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += ScrollIntoView;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= ScrollIntoView;
            base.OnDetaching();
        }

        private void ScrollIntoView(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = (ListBox) sender;
            if(listBox?.SelectedItem == null)
                return;

            var item = (ListBoxItem) listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem);
            item?.BringIntoView();
        }
    }
}
