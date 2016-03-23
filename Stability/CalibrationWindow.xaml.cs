using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Stability.Enums;
using Stability.Model;
using Stability.Model.Port;
using Stability.View;

namespace Stability
{
    /// <summary>
    /// Interaction logic for CalibrationWindow.xaml
    /// </summary>
    public partial class CalibrationWindow : IView
    {
        private readonly double _butH;
        private readonly double _butW;
       
        private double[] _weightKoefs;
        private CalibratePresenter _presenter;

        private CalibrationParams _calibrationParams = new CalibrationParams();
        private TenzoRadioButton[] arr;
        private ButtonHandler buttonHandler;

        public CalibrationWindow(IStabilityModel model)
        {
            InitializeComponent();
            buttonHandler = new ButtonHandler(but_ok.Height,but_ok.Width);
           /* _butH = but_ok.Height;
            _butW = but_ok.Width;*/
            arr = new TenzoRadioButton[4] { Tenz0, Tenz1, Tenz2, Tenz3 };
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if(i!=j)
                        arr[i].GroupTenzoRadioButtons.Add(arr[j]);
                }
            }
            Tenz0.IsChecked = true;

            _weightKoefs = MainConfig.WeightKoefs;
            _tenz0_Koef.Text = _weightKoefs[0].ToString();
            _tenz1_Koef.Text = _weightKoefs[1].ToString();
            _tenz2_Koef.Text = _weightKoefs[2].ToString();
            _tenz3_Koef.Text = _weightKoefs[3].ToString();

            _calibrationParams.EntryCount = 100;
            _calibrationParams.Weight = 7.5;

           _presenter = new CalibratePresenter(model,this);
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

        private void UpdateParams()
        {
            _editWeight_LostFocus(this, null);
            editEntryCount_LostFocus(this,null);
        }
        private bool CheckParams()
        {
            if (_calibrationParams.Weight < 5.0)
            {
                MessageBox.Show(this, "Значение веса слишком мало, груз должен быть более 5кг.", "Малый вес",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (_calibrationParams.EntryCount < 10)
            {
                MessageBox.Show(this, "Количество записей должно быть больше 10", "Слишком мало записей",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (_calibrationParams.EntryCount > 2000)
            {
                MessageBox.Show(this, "Количество записей не должно превышать 2000", "Слишком много записей",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateParams();
            if(CheckParams())
                DeviceCmdEvent.Invoke(this,new DeviceCmdArgEvent(){cmd = DeviceCmd.WIEGHT_CALIBRATE, Params = _calibrationParams});
        }

        private void but_MouseEnter(object sender, MouseEventArgs e)
        {
            buttonHandler.but_MouseEnter(sender,e);
        }

        private void but_MouseLeave(object sender, MouseEventArgs e)
        {
            buttonHandler.but_MouseLeave(sender,e);
        }
        /*
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
        */
        public void UpdateTenzView(string[] tenz)
        {
            Dispatcher.BeginInvoke(new Action(() => _tenz0_Koef.Text = tenz[0]));
            Dispatcher.BeginInvoke(new Action(() => _tenz1_Koef.Text = tenz[1]));
            Dispatcher.BeginInvoke(new Action(() => _tenz2_Koef.Text = tenz[2]));
            Dispatcher.BeginInvoke(new Action(() => _tenz3_Koef.Text = tenz[3]));
            _weightKoefs = _presenter.CurrWeightKoefs;
        }

        public void COnPortStatusChanged(object sender, PortStatusChangedEventArgs portStatusChangedEventArgs)
        {
            return;
        }

        public event EventHandler ViewUpdated;
        public event EventHandler<DeviceCmdArgEvent> DeviceCmdEvent;

        private void comboPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int[] periods = { 30, 40, 50, 100, 150, 200 };

            var v = ((ComboBox) sender).SelectedIndex;
            _calibrationParams.Period = periods[v];
        }

        private void _editWeight_LostFocus(object sender, RoutedEventArgs e)
        {
            double var;

            if (!Double.TryParse(_editWeight.Text, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-GB"), out var))
            {
                MessageBox.Show(this, "Значение веса ввдено неверно!", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
                _calibrationParams.Weight = var;
        }

        private void editEntryCount_LostFocus(object sender, RoutedEventArgs e)
        {
            int v1;
            if (!int.TryParse(editEntryCount.Text, out v1))
            {
                MessageBox.Show(this, "Количество записей ввдено неверно!", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else
             _calibrationParams.EntryCount = v1;
        }

        private void Tenz_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
           for (byte i = 0; i < 4; i++)
                if (arr[i].IsChecked)
                {
                    _calibrationParams.TenzNumber = i;
                    break;
                }
        }

        private void next_tenz_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var n = _calibrationParams.TenzNumber;
            if (++n == 4) n = 0;
            _calibrationParams.TenzNumber = n;
            arr[n].IsChecked = true;
        }

        private void prev_tenz_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var n = _calibrationParams.TenzNumber;
            if (--n == byte.MaxValue) n = 3;
            _calibrationParams.TenzNumber = n;
            arr[n].IsChecked = true;
        }

        private void SaveBut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainConfig.Update(_weightKoefs,null);
          //  MainConfig.Update(_presenter.CurrWeightKoefs,null);
        }
    }

   public class ButtonHandler
    {
        private readonly double _butH;
        private readonly double _butW;

       public ButtonHandler(double H, double W)
       {
           _butH = H;
           _butW = W;
       }

        public void but_MouseEnter(object sender, MouseEventArgs e)
        {
            var w = new DoubleAnimation();
            var h = new DoubleAnimation();
            w.From = _butW;
            w.To = _butW + 5;
            w.Duration = TimeSpan.FromMilliseconds(20);

            h.From = _butH;
            h.To = _butH + 5;
            h.Duration = TimeSpan.FromMilliseconds(20);

            ((Image)sender).BeginAnimation(Image.WidthProperty, w);
            ((Image)sender).BeginAnimation(Image.HeightProperty, h);

            //  var op = new DoubleAnimation(0.0, 100.0, TimeSpan.FromMilliseconds(1));
            //  but_ok.Effect.BeginAnimation(DropShadowEffect.OpacityProperty,op);//SetValue(OpacityProperty,100.0);
            ((Image)sender).Effect.SetCurrentValue(DropShadowEffect.OpacityProperty, 100.0);
            //  but_ok.Effect.SetValue();
        }

        public void but_MouseLeave(object sender, MouseEventArgs e)
        {
            var a = new DoubleAnimation();
            var h = new DoubleAnimation();
            a.From = _butW + 5;//but_ok.Width;
            a.To = _butW;
            a.Duration = TimeSpan.FromMilliseconds(20);

            h.From = _butH + 5;//but_ok.Height;
            h.To = _butH;
            h.Duration = TimeSpan.FromMilliseconds(20);

            ((Image)sender).BeginAnimation(Image.WidthProperty, a);
            ((Image)sender).BeginAnimation(Image.HeightProperty, h);

            ((Image)sender).Effect.SetCurrentValue(DropShadowEffect.OpacityProperty, 0.0);
        }
    }
}
