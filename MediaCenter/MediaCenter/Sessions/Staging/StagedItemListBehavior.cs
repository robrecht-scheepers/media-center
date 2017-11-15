using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace MediaCenter.Sessions.Staging
{
    public class StagedItemListBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += AssociatedObjectOnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= AssociatedObjectOnSelectionChanged;
        }

        private void AssociatedObjectOnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            SetStagedItemViewButtons((ListBox)sender, args.RemovedItems, Visibility.Collapsed);
            SetStagedItemViewButtons((ListBox)sender, args.AddedItems, Visibility.Visible);
        }

        private static void SetStagedItemViewButtons(DependencyObject root, System.Collections.IList items, Visibility visibility)
        {
            if(root == null)
                return;

            var stagedItem = root as StagedItemView;
            if (stagedItem != null)
            {
                if(items.Contains(stagedItem.DataContext))
                    ((StagedItemView) root).ButtonList.Visibility = visibility;
                return;
            }
            else
            {
                var numChildren = VisualTreeHelper.GetChildrenCount(root);
                if (numChildren > 0)
                {
                    for (int i = 0; i < numChildren; i++)
                        SetStagedItemViewButtons(VisualTreeHelper.GetChild(root, i), items, visibility);
                }
            }
        }

        private static int FindStagedItemsRootLevel(DependencyObject root, Type type)
        {
            return 0;
        }
    }
}
