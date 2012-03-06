using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using SuperSocket.Management.Client.Model;

namespace SuperSocket.Management.Client.Selector
{
    public class InstanceRowStyleSelector : StyleSelector
    {
        public Style NormalStyle { get; set; }

        public Style LoadingStyle { get; set; }

        public override Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is LoadingInstanceModel)
                return LoadingStyle;
            else
                return NormalStyle;
        }
    }
}
