using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;
using Stability.Enums;
using Stability.Model;
using Stability.Model.Device;
using Stability.Model.Port;
using Stability.View;

namespace Stability
{
    /// <summary>
    /// Interaction logic for DataRxWindow.xaml
    /// </summary>
    public partial class DataRxWindow : IView
    {
        private ButtonHandler buttonHandler;
        private double[] w_koefs;
        private DataRxWinPresenter _presenter;
        private readonly int[] _periods = { 30, 40, 50, 100, 150, 200 };
        public DataRxWindow(IStabilityModel model)
        {
            InitializeComponent();
            buttonHandler = new ButtonHandler(but_ok.Width,but_ok.Height);
            w_koefs = MainConfig.ExchangeConfig.AlphaBetaKoefs;

            TextBox_W1.Text = w_koefs[0].ToString(CultureInfo.CreateSpecificCulture("en-GB"));
            TextBox_W2.Text = w_koefs[1].ToString(CultureInfo.CreateSpecificCulture("en-GB"));
            TextBox_W3.Text = w_koefs[2].ToString(CultureInfo.CreateSpecificCulture("en-GB"));
            TextBox_W4.Text = w_koefs[3].ToString(CultureInfo.CreateSpecificCulture("en-GB"));

            var pnl = SerialPort.GetPortNames().ToList();
            combo_portName.ItemsSource = pnl;
            if (pnl.Contains(MainConfig.PortConfig.PortName))
                combo_portName.SelectedValue = MainConfig.PortConfig.PortName;
            else
            {
                pnl.Add("NoCOM");
                combo_portName.SelectedValue = "NoCOM";
                MessageBox.Show(this, "Требуемый порт не обнаружен. Выполните поиск порта, или укажите его вручную.",
                    "Ошибка. Порт отсутствует.", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            combo_RxPeriod.SelectedIndex = GetPeriodIndex();
            check_AutoConnect.IsChecked = MainConfig.PortConfig.AutoConnect;
            check_SavePureADCs.IsChecked = MainConfig.ExchangeConfig.SavePureADCs;
            check_CorrectMistakes.IsChecked = MainConfig.ExchangeConfig.CorrectRxMistakes;
            combo_RxFilterType.SelectedIndex = (int) MainConfig.ExchangeConfig.FilterType;
            _presenter = new DataRxWinPresenter(model,this);
        }

        private int GetPeriodIndex()
        {
            int i;
            for (i = 0; i < _periods.Count(); i++)
                if (_periods[i] == MainConfig.ExchangeConfig.Period)
                    break;
            return i;
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

            //ViewUpdated.Invoke(this,null);
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
            but_ok.Focus();
            CPortConfig portConf;
            StabilityExchangeConfig exchConf;
            GetWinState(out portConf, out exchConf);
            MainConfig.Update(exchConf);
            MainConfig.Update(portConf);
          
            MessageBox.Show(this, "Новые параметры успешно сохранены", "Сохранено", MessageBoxButton.OK,
                   MessageBoxImage.Information);
        }

        private void combo_RxFilterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if((combo_RxFilterType == null)||(group_FilterWs == null)) return;

            if(combo_RxFilterType.SelectedIndex == 1)
                group_FilterWs.Visibility = Visibility.Visible;
            else
                group_FilterWs.Visibility = Visibility.Hidden;

            if (ViewUpdated != null)
                ViewUpdated.Invoke(this, null);
        }

        private void _editW_PreviewTextInput(object sender, TextCompositionEventArgs e)
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

        private void _editW_LostFocus(object sender, RoutedEventArgs e)
        {
            double var;

            int n;
            var s = ((TextBox) sender).Name.Replace("TextBox_W", "");
            int.TryParse(s,out n);
            n--;

            if (!Double.TryParse(((TextBox) sender).Text, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-GB"),
                out var))
            {
                MessageBox.Show(this, "Значение веса ввдено неверно!", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
           else
            {
                if (var > 1.0)
                {
                    ((TextBox) sender).Text = "1";
                    var = 1;
                }
                w_koefs[n] = var;

                if (ViewUpdated != null)
                    ViewUpdated.Invoke(this, null);
            }
        }

        public void GetWinState(out CPortConfig c, out StabilityExchangeConfig c1)
        {
            var name = combo_portName.SelectedValue;
            
           c = new CPortConfig(){AutoConnect = (bool) check_AutoConnect.IsChecked, Baud = MainConfig.PortConfig.Baud, UseSLIP = true,PortName = (string) name};

           var n = combo_RxPeriod.SelectedIndex;
           c1 = new StabilityExchangeConfig
           {
               FilterType = (InputFilterType) combo_RxFilterType.SelectedIndex,
               SavePureADCs = (bool) check_SavePureADCs.IsChecked,
               CorrectRxMistakes = (bool) check_CorrectMistakes.IsChecked,
               Period = _periods[n],
               AlphaBetaKoefs = w_koefs
           };
        }

        public void UpdateTenzView(string[] tenz)
        {
            throw new NotImplementedException();
        }

        public void COnPortStatusChanged(object sender, PortStatusChangedEventArgs portStatusChangedEventArgs)
        {
            throw new NotImplementedException();
        }

        public void UpdatePatientData(PatientModelResponseArg patientModelResponseArg)
        {
            throw new NotImplementedException();
        }

        public void UpdateAnamnesisData(AnamnesisModelResponseArg anamnesisModelResponseArg)
        {
            throw new NotImplementedException();
        }

        public void UpdateDataInGridRes(DeviceDataEntry d)
        {
            throw new NotImplementedException();
        }

        public void UpdateButtons()
        {
            throw new NotImplementedException();
        }

        public void UpdateProgress(ProgressEventArgs progress)
        {
            throw new NotImplementedException();
        }

        public event EventHandler ViewUpdated;
        
        public event EventHandler<DeviceCmdArgEvent> DeviceCmdEvent;
        public event EventHandler<PatientModelResponseArg> PatientEvent;
        public event EventHandler<AnamnesisModelResponseArg> AnamnesisEvent;
        public event EventHandler<AnalyzerCmdResponseArg> AnalyzerEvent;
        public event EventHandler<EventArgs> ResultUpdEvent;

        private void combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          if (ViewUpdated != null)
            ViewUpdated.Invoke(this, null);
        }

        private void checks_Click(object sender, RoutedEventArgs e)
        {
            if (ViewUpdated != null)
                ViewUpdated.Invoke(this, null);
        }

        private void but_find_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
          string name;
          if (!CComPort.FindPort(portTemplate.Text, out name))
                MessageBox.Show(this, "Требуемый порт не обнаружен. Проверьте правильность строки с именем порта. " +
                                      "Также удостоверьтесь, что устройство подключено к компьютеру.",
                    "Ошибка. Порт не найден.", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            else
                combo_portName.SelectedValue = name;
        }

        private void zeroCalib_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(this, "Освободите платформу от нагрузки перед началом калибровки", "Внимание!",
                MessageBoxButton.OK, MessageBoxImage.Warning);

           if (DeviceCmdEvent != null)
             DeviceCmdEvent.Invoke(this, new DeviceCmdArgEvent() { cmd = DeviceCmd.ZERO_CALIBRATE });
        }
    }
}
