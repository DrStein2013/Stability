using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for SimpleWindow.xaml
    /// </summary>
    public partial class SimpleWindow : Window
    {
        public double Value { get; private set; }

        public SimpleWindow(string Caption, string LabelText)
        {
            InitializeComponent();
            Title = Caption;
            Label.Content = LabelText;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            double var;

            if (
                !Double.TryParse(textBox_input.Text, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-GB"),
                    out var))
            {
                MessageBox.Show(this, "Значение веса ввдено неверно!", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
            {
                DialogResult = true;
                Value = var;
            }
            
        }

        private void TextBox_input_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
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


    }
}
