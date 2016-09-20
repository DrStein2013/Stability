using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Stability.Annotations;
using Stability.Model;

namespace Stability
{
    /// <summary>
    /// Interaction logic for ResearchResults.xaml
    /// </summary>
    public partial class ResearchResults : Window
    {
        
    public ResearchResults(IStabilityModel model)
        {
           InitializeComponent();
        var n = new ResearchResultPresenter(model) {P = 15};
        this.DataContext = n;
        }

       
    }
}
