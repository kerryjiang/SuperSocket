using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SuperSocket.Management.Client.ViewModel;

namespace SuperSocket.Management.Client
{
    public class InstanceGroupDescription : GroupDescription
    {
        public override object GroupNameFromItem(object item, int level, System.Globalization.CultureInfo culture)
        {
            var instance = item as InstanceViewModelBase;
            return instance.Server;
        }
    }
}
