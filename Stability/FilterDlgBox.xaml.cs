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
using Stability.Enums;

namespace Stability
{
    /// <summary>
    /// Interaction logic for FilterDlgBox.xaml
    /// </summary>
    public partial class FilterDlgBox : Window
    {
        public FilterType FlType { get; private set; }
        public int WindowFlt { get; private set; }
        public FilterDlgBox()
        {
            InitializeComponent();
            FlType = FilterType.MovingAverage;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var c = ((ComboBox) sender);
            FlType = (FilterType) c.SelectedIndex;
        }

        private void text_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
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
            Int16 h=0;
            string res = null;
            
            if (Int16.TryParse(text_WinFlt.Text, out h))
            {
                if (h == 0)
                    res = "Окно не может равняться нулю";
                else if (h > 100)
                    res = "Окно не может быть больше 100 элементов";
                else 
                    WindowFlt = h;
            }
            else
                res = "Значение заполнено неверно!";

            if (res != null)
            {
                MessageBox.Show(this, res, "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
              DialogResult = true;
            
        }


    }
}
