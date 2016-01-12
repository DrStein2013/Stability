using System;
using System.Collections.Generic;
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
            c = new CComPort("COM1");
            if (n)
            {

                var conf = new CPortConfig() {PortName = name, AutoConnect = true, Baud = 9600};
                c = new CComPort(conf);
                /*c = new CComPort(name) {AutoConnect = true};
                n = c.Connect(out name);*/
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            c.Test(1);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            c.Test(2);
        }
    }
}
