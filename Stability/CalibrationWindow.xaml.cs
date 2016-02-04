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
        private readonly double _butH;
        private readonly double _butW;
        private double[] _weightKoefs;
        public CalibrationWindow()
        {
            InitializeComponent();
            _butH = but_ok.Height;
            _butW = but_ok.Width;
            var arr = new TenzoRadioButton[4] { Tenz0, Tenz1, Tenz2, Tenz3 };
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if(i!=j)
                        arr[i].GroupTenzoRadioButtons.Add(arr[j]);
                }
            }

            _weightKoefs = MainConfig.WeightKoefs;
            _tenz0_Koef.Text = _weightKoefs[0].ToString();
            _tenz1_Koef.Text = _weightKoefs[1].ToString();
            _tenz2_Koef.Text = _weightKoefs[2].ToString();
            _tenz3_Koef.Text = _weightKoefs[3].ToString();
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
            w.From = _butW;
            w.To = _butW + 5;
            w.Duration = TimeSpan.FromMilliseconds(20);

            h.From = _butH;
            h.To = _butH + 5;
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
            a.To = _butW;
            a.Duration = TimeSpan.FromMilliseconds(20);

            h.From = but_ok.Height;
            h.To = _butH;
            h.Duration = TimeSpan.FromMilliseconds(20);

            ((Image)sender).BeginAnimation(WidthProperty, a);
            ((Image)sender).BeginAnimation(HeightProperty, h);

            ((Image)sender).Effect.SetCurrentValue(DropShadowEffect.OpacityProperty, 0.0);
        }

   

    }
}
