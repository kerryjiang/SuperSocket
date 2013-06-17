using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Interactivity;
using System.Linq;
using System.Windows.Data;

namespace SuperSocket.Management.AgentClient.Behaviors
{
    public class SelectRowOnRightClickBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.LoadingRow += new EventHandler<DataGridRowEventArgs>(AssociatedObject_LoadingRow);
            AssociatedObject.UnloadingRow += new EventHandler<DataGridRowEventArgs>(AssociatedObject_UnloadingRow);
        }

        void AssociatedObject_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseRightButtonDown -= new MouseButtonEventHandler(Row_MouseRightButtonDown);
        }

        void AssociatedObject_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var contextMenu = new ContextMenu();

            contextMenu.DataContext = e.Row.DataContext;

            var newMenuItem = new MenuItem();
            newMenuItem.Header = "Start";
            newMenuItem.SetBinding(MenuItem.CommandProperty, new Binding("[StartCommand]"));
            newMenuItem.SetBinding(MenuItem.CommandParameterProperty, new Binding());
            contextMenu.Items.Add(newMenuItem);

            newMenuItem = new MenuItem();
            newMenuItem.Header = "Stop";
            newMenuItem.SetBinding(MenuItem.CommandProperty, new Binding("[StopCommand]"));
            newMenuItem.SetBinding(MenuItem.CommandParameterProperty, new Binding());
            contextMenu.Items.Add(newMenuItem);

            e.Row.MouseRightButtonDown += new MouseButtonEventHandler(Row_MouseRightButtonDown);
            ContextMenuService.SetContextMenu(e.Row, contextMenu);
        }

        void Row_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var elementsUnderMouse = VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(null), AssociatedObject);

            var row = elementsUnderMouse
                .OfType<DataGridRow>()
                .FirstOrDefault();

            if (row != null)
            {
                if (AssociatedObject.SelectedItem != row.DataContext)
                {
                    AssociatedObject.SelectedItem = row.DataContext;
                }
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.LoadingRow -= new EventHandler<DataGridRowEventArgs>(AssociatedObject_LoadingRow);
        }
    }
}
