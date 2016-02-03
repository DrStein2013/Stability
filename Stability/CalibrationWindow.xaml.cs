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
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Stability
{
    /// <summary>
    /// Interaction logic for CalibrationWindow.xaml
    /// </summary>
    public partial class CalibrationWindow : Window
    {
        private double but_h;
        private double but_w;
        public CalibrationWindow()
        {
            InitializeComponent();
            but_h = but_ok.Height;
            but_w = but_ok.Width;
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

        private void but_MouseEnter(object sender, MouseEventArgs e)
        {
            var w = new DoubleAnimation();
            var h = new DoubleAnimation();
            w.From = but_w;
            w.To = but_w + 5;
            w.Duration = TimeSpan.FromMilliseconds(20);

            h.From = but_h;
            h.To = but_h + 5;
            h.Duration = TimeSpan.FromMilliseconds(20);

            ((Image)sender).BeginAnimation(WidthProperty, w);
            ((Image)sender).BeginAnimation(HeightProperty, h);

          //  var op = new DoubleAnimation(0.0, 100.0, TimeSpan.FromMilliseconds(1));
          //  but_ok.Effect.BeginAnimation(DropShadowEffect.OpacityProperty,op);//SetValue(OpacityProperty,100.0);
            ((Image)sender).Effect.SetCurrentValue(DropShadowEffect.OpacityProperty, 100.0);
          //  but_ok.Effect.SetValue();
        }

        private void but_MouseLeave(object sender, MouseEventArgs e)
        {
            var a = new DoubleAnimation();
            var h = new DoubleAnimation();
            a.From = but_ok.Width;
            a.To = but_w;
            a.Duration = TimeSpan.FromMilliseconds(20);

            h.From = but_ok.Height;
            h.To = but_h;
            h.Duration = TimeSpan.FromMilliseconds(20);

            ((Image)sender).BeginAnimation(WidthProperty, a);
            ((Image)sender).BeginAnimation(HeightProperty, h);

            ((Image)sender).Effect.SetCurrentValue(DropShadowEffect.OpacityProperty, 0.0);
        }

   

    }
}
