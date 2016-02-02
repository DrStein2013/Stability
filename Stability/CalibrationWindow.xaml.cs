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
    /// Interaction logic for CalibrationWindow.xaml
    /// </summary>
    public partial class CalibrationWindow : Window
    {
        public CalibrationWindow()
        {
            InitializeComponent();
            var arr = new TenzoRadioButton[4] { Tenz0, Tenz1, Tenz2, Tenz3 };
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if(i!=j)
                        arr[i].GroupTenzoRadioButtons.Add(arr[j]);
                }
            }

        }

        private void _editWeight_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (((e.Text == ".") && (!((TextBox)sender).Text.Contains("."))) || (Char.IsDigit(e.Text, 0)))
                    e.Handled = false;
            }
            catch
            {
                e.Handled = true;
            }      
        }

        private void editEntryCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (Char.IsDigit(e.Text, 0))
                    e.Handled = false;
            }
            catch
            {
                e.Handled = true;
            }      
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bar.Value += 1;
        }
    }
}
