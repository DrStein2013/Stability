﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Stability.Enums;
using Stability.Model;
using Stability.Model.Device;
using Stability.Model.Port;
using Stability.View;

namespace Stability
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IView
    {
        private StabilityPresenter _presenter;
        public MainWindow()
        {
            InitializeComponent();
            _presenter = new StabilityPresenter(new StabilityModel(), this);
            
            Thread.Sleep(200);
           // Button_Click_1(this,null);
        }

        public void UpdateTenzView(string[] tenz)
        {
                Dispatcher.BeginInvoke(new Action(() => Tenz0.Text = tenz[0]));
                Dispatcher.BeginInvoke(new Action(() => Tenz1.Text = tenz[1])); 
                Dispatcher.BeginInvoke(new Action(() => Tenz2.Text = tenz[2]));
                Dispatcher.BeginInvoke(new Action(() => Tenz3.Text = tenz[3]));
        }

        public void COnPortStatusChanged(object sender, PortStatusChangedEventArgs portStatusChangedEventArgs)
        {
            if(portStatusChangedEventArgs.Status == EPortStatus.Open)
                Dispatcher.BeginInvoke(new Action(() => StatusMark.Fill = new SolidColorBrush(Colors.Green)));
            else if(portStatusChangedEventArgs.Status == EPortStatus.Closed)
                Dispatcher.BeginInvoke(new Action(() => StatusMark.Fill = new SolidColorBrush(Colors.Red)));
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


        private void Discon_OnClick(object sender, RoutedEventArgs e)
        {
            IoC.Resolve<IPort>().Disconnect();
        }
        
        private void Con_OnClick(object sender, RoutedEventArgs e)
        {
            string s;
            var c = IoC.Resolve<IPort>();
             if (!c.Connect(out s))
                MessageBox.Show(this, s, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        public event EventHandler ViewUpdated;
        public event EventHandler<DeviceCmdArgEvent> DeviceCmdEvent;

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (DeviceCmdEvent != null)
                DeviceCmdEvent.Invoke(this,new DeviceCmdArgEvent(){cmd = DeviceCmd.START_MEASURE});
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (DeviceCmdEvent != null)
                DeviceCmdEvent.Invoke(this, new DeviceCmdArgEvent() { cmd = DeviceCmd.STOP_MEASURE });
        }

        private void OnClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
           Button_Click_2(this,null);
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            var a = new DoubleAnimation
            {
                From = gr1.ActualHeight,
                To = exp1.ActualHeight,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            gr1.BeginAnimation(HeightProperty, a);
        }

        private void exp1_Collapsed(object sender, RoutedEventArgs e)
        {
            gr1.SetCurrentValue(HeightProperty,0.0);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceCmdEvent != null)
                DeviceCmdEvent.Invoke(this, new DeviceCmdArgEvent() { cmd = DeviceCmd.ZERO_CALIBRATE });
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if(DeviceCmdEvent!=null)
            { DeviceCmdEvent.Invoke(this, new DeviceCmdArgEvent() { cmd = DeviceCmd.WEIGHT_MEASURE }); }
//            var win = new CalibrationWindow(_presenter.Model);
//            win.ShowDialog();
        }

        private void DataRxItem_OnClick(object sender, RoutedEventArgs e)
        {
            var win = new DataRxWindow(_presenter.Model);
            win.ShowDialog();
            
        }

        private void WeightCalibItem_OnClick(object sender, RoutedEventArgs e)
        {
            var win = new CalibrationWindow(_presenter.Model);
            win.ShowDialog();
        }

        private void StartUpCalibItem_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceCmdEvent != null)
                DeviceCmdEvent.Invoke(this, new DeviceCmdArgEvent() { cmd = DeviceCmd.STARTUP_CALIBRATE });
        }

        private void FastWeightCalibItem_Click(object sender, RoutedEventArgs e)
        {
           var win = new SimpleWindow("Вес для калибровки","Введите значения веса для калибровки:");
    
           if((bool)win.ShowDialog())
             if (DeviceCmdEvent != null)
                DeviceCmdEvent.Invoke(this, new DeviceCmdArgEvent() { cmd = DeviceCmd.WEIGHT_CALIBRATE_FAST,Params = new CalibrationParams(){Weight = win.Value,EntryCount = 100}});
        }

        private void WeightParamItem_OnClick(object sender, RoutedEventArgs e)
        {
            
        }
    }

}

