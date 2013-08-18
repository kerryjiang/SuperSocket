using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows.Controls;
using SuperSocket.ServerManager.Client.Command;

namespace SuperSocket.ServerManager.Client.Behaviors
{
    public class ContextMenuBehavior : Behavior<ContextMenu>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.Opened += new System.Windows.RoutedEventHandler(AssociatedObject_Opened);
            base.OnAttached();
        }

        void AssociatedObject_Opened(object sender, System.Windows.RoutedEventArgs e)
        {
            var contextMenu = this.AssociatedObject;

            foreach (var menuItem in contextMenu.Items.OfType<MenuItem>())
            {
                ((DelegateCommand<DynamicViewModel.DynamicViewModel>)menuItem.Command).RaiseCanExecuteChanged();
            }
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.Opened -= new System.Windows.RoutedEventHandler(AssociatedObject_Opened);
            base.OnDetaching();
        }
    }
}
