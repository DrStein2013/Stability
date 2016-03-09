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
using System.Windows.Shapes;

namespace Stability
{
    /// <summary>
    /// Interaction logic for DataRxWindow.xaml
    /// </summary>
    public partial class DataRxWindow : Window
    {
        private ButtonHandler buttonHandler;

        public DataRxWindow()
        {
            InitializeComponent();
            buttonHandler = new ButtonHandler(but_ok.Width,but_ok.Height);
        }

        private void but_MouseEnter(object sender, MouseEventArgs e)
        {
            buttonHandler.but_MouseEnter(sender,e);
        }

        private void but_MouseLeave(object sender, MouseEventArgs e)
        {
            buttonHandler.but_MouseLeave(sender,e);
        }

        private void Button_Click(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            if (combo_portName == null) return;

            combo_portName.IsEnabled = false;
            portTemplate.IsEnabled = false;
            but_find.IsEnabled = false;
        }

        private void CheckBox_Unchecked_1(object sender, RoutedEventArgs e)
        {
            if (combo_portName == null) return;

            combo_portName.IsEnabled = true;
            portTemplate.IsEnabled = true;
            but_find.IsEnabled = true;
        }

        private void but_ok_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void combo_RxFilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if((combo_RxFilterType == null)||(group_FilterWs == null)) return;

            if(combo_RxFilterType.SelectedIndex == 1)
                group_FilterWs.Visibility = Visibility.Visible;
            else
                group_FilterWs.Visibility = Visibility.Hidden;    
        }

    }
}
