using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Stability.Model.Port;

namespace Stability.View
{
    interface IView
    {
        void UpdateView();
        void COnPortStatusChanged(object sender, PortStatusChangedEventArgs portStatusChangedEventArgs);

        event EventHandler ViewUpdated;
    }
}
