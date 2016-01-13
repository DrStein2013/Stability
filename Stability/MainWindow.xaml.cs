using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Stability.Model;
using Stability.Model.Port;

namespace Stability
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CComPort c;
        public MainWindow()
        {
            InitializeComponent();
            string name;
            var n = CComPort.FindPort("Stabilometric Device", out name);
            if (n)
            {

                var conf = new CPortConfig() {PortName = name, AutoConnect = true, Baud = 9600};
                c = new CComPort(conf);
            }
        }
        private void OnExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_OnChecked(object sender, RoutedEventArgs e)
        {
            Con.Visibility = Visibility.Collapsed;
            Discon.Visibility = Visibility.Collapsed;
        }

        private void MenuItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Con.Visibility = Visibility.Visible;
            Discon.Visibility = Visibility.Visible;
        }
    }
}
