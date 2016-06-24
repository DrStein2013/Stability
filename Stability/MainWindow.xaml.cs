using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Stability.Enums;
using Stability.Model;
using Stability.Model.Device;
using Stability.Model.Port;
using Stability.View;
using ZedGraph;
using Color = System.Windows.Media.Color;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;
using ComboBox = System.Windows.Controls.ComboBox;

namespace Stability
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IView
    {
        private StabilityPresenter _presenter;
        private bool isStarted = false;

        public MainWindow()
        {
            InitializeComponent();
            _presenter = new StabilityPresenter(new StabilityModel(), this);
            
            Thread.Sleep(200);
            GraphPane pane = testGraph.GraphPane;


            pane.Chart.Fill.Brush = new SolidBrush(System.Drawing.Color.LightGray);
            pane.Fill.Color = System.Drawing.Color.LightGray;
            // Очистим список кривых на тот случай, если до этого сигналы уже были нарисованы
            pane.CurveList.Clear();

            pane.AddCurve("Tenz0", new PointPairList(), System.Drawing.Color.Green, SymbolType.None);
            pane.AddCurve("Tenz1", new PointPairList(), System.Drawing.Color.Blue, SymbolType.None);
            pane.AddCurve("Tenz2", new PointPairList(), System.Drawing.Color.Red, SymbolType.None);
            pane.AddCurve("Tenz3", new PointPairList(), System.Drawing.Color.Orange, SymbolType.None);

            pane.XAxis.Scale.Max = 30;
            pane.XAxis.Scale.MajorStep = 10;
            pane.XAxis.Scale.MinorStep = 2;
            pane.XAxis.Title.Text = "Время, сек";
            pane.YAxis.Title.Text = "Значение, V";
            pane.Title.Text = "Показания тензодатчиков";

            //   buttonHandler = new ButtonHandler(but_ok.Height, but_ok.Width);
            //     but_ok.MouseEnter += buttonHandler.But_MouseEnter;
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

        public void UpdatePatientData(PatientModelResponseArg patientModelResponseArg)
        {
            var Img = patientModelResponseArg.Error ? MessageBoxImage.Error : MessageBoxImage.Information;
            var Cap = patientModelResponseArg.Error ? "Ошибка" : "Готово";
            if (!patientModelResponseArg.Error)
            {
                grid.ItemsSource = patientModelResponseArg.PatientTable;
                form_Pat.SetToForm(patientModelResponseArg.Patient);
            }
            text_ID.Text = patientModelResponseArg.ID.ToString();
            text_ID.Foreground = new SolidColorBrush((Color)text_ID.Resources["TextColorBlack"]);
            if(patientModelResponseArg.Response!=null)
                MessageBox.Show(patientModelResponseArg.Response, Cap, MessageBoxButton.OK,Img);
        }

        public void UpdateAnamnesisData(AnamnesisModelResponseArg anamnesisModelResponseArg)
        {
            switch (anamnesisModelResponseArg.Action)
            {
                case BaseAction.ClearEntry:
                    if (anamnesisModelResponseArg.EntryState == BaseEntryState.New)
                    {

                        var res = MessageBox.Show(anamnesisModelResponseArg.Response, "Сохранить?",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (res == MessageBoxResult.Yes)
                            but_save_Click(this, null);
                        else
                        {
                            anamnesisModelResponseArg.EntryState = BaseEntryState.Empty;
                            AnamnesisEvent.Invoke(this, anamnesisModelResponseArg);
                            ClearGridGraph();
                        }
                    } 
                 break;
                case BaseAction.Find:
                    if (anamnesisModelResponseArg.Error)
                        MessageBox.Show(anamnesisModelResponseArg.Response, "Ошибка", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    else
                        grid_Anam.ItemsSource = anamnesisModelResponseArg.Table;
                    break;
            }
            
           // var Img = anamnesisModelResponseArg.Error ? MessageBoxImage.Information : MessageBoxImage.Information;
           // var Cap = anamnesisModelResponseArg.Error ? "Нет записей" : "Готово";
           /* if (anamnesisModelResponseArg.Error)
            {
                if (anamnesisModelResponseArg.Action == BaseAction.ClearEntry)
                {
                    
                }
                else if (anamnesisModelResponseArg.Response != null)
                    MessageBox.Show(anamnesisModelResponseArg.Response, Cap, MessageBoxButton.OK, Img);
            }
            grid_Anam.ItemsSource = anamnesisModelResponseArg.Table;*/
        }

        private void ClearGridGraph()
        {
            grid_Result.ItemsSource = null;
            var crvList = testGraph.GraphPane.CurveList;
            crvList.ForEach(item => item.Clear());
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

        public void UpdateButtons()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                but_stop.Visibility = Visibility.Collapsed;
                but_rec.Visibility = Visibility.Visible;
            }));
        }

        public void UpdateProgress(ProgressEventArgs progress)
        {
            Dispatcher.BeginInvoke(new Action(()=> bar_meas.Value = progress.EntryCount));
            Dispatcher.BeginInvoke(new Action(() => text_Timer.Text = progress.TimerCount.ToString("f1")));
            Dispatcher.BeginInvoke(new Action(delegate
            {
                var curv = testGraph.GraphPane.CurveList;
                for (int i =0;i<progress.Vals.Count();i++)
                {
                    curv[i].AddPoint(progress.TimerCount, progress.Vals[i]);
                }
                testGraph.AxisChange();
                testGraph.Invalidate();
            }));
        }

        public event EventHandler ViewUpdated;
        public event EventHandler<DeviceCmdArgEvent> DeviceCmdEvent;
        public event EventHandler<PatientModelResponseArg> PatientEvent;
        public event EventHandler<AnamnesisModelResponseArg> AnamnesisEvent;
        public event EventHandler<AnalyzerCmdResponseArg> AnalyzerEvent;
        public event EventHandler<EventArgs> LoadDataEntry;

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
           //DrawGraph();
        }

        private void Test1_OnClick(object sender, RoutedEventArgs e)
        {
            var l = new List<double[]>();
            for (int i = 0; i < 1200; i++)
            {
                l.Add(new double[] { 5, 7, 8, 9 });
            }

            var t = new DeviceDataEntry(l);
            var Base = new cDataBase();
            var an = new cAnamnesisEntry() { Entry = t, Info = "ямайский музыкант, регги-исполнитель, обладатель 3-х наград «Грэмми». Дэмиан — младший сын легендарного Боба Марли. Его мать — Синди Брэйкспир, обладательница титула «Мисс Мира ’76». Своё прозвище 'Junior Gong' Дэмиан получил от прозвища отца — 'Tuff Gong'. Дэмиан выступает с 13 лет. Также, как и большая часть его семьи, он полностью посвятил себя музыке. ", Weight = 59 };
            Base.AddAnamnesis(an,1);
        }

        private void Test2_OnClick(object sender, RoutedEventArgs e)
        {
            var adp = new PatientBaseDataSetTableAdapters.AnamnesisTableAdapter();
            var t = adp.GetDataBy(0);
            grid_Anam.ItemsSource = t;
            var b_arr = t[0].Entries;

          /*  var bin_format = new BinaryFormatter();
            var mem_stream = new MemoryStream();
            mem_stream.Write(b_arr,0,b_arr.Count());
            mem_stream.Position = 0;
            DeviceDataEntry d = (DeviceDataEntry) bin_format.Deserialize(mem_stream);
           */

        }

        private void Test3_OnClick(object sender, RoutedEventArgs e)
        {
            string res;
            text_ID.Text = "";
            text_ID_LostFocus(text_ID,null);
            var pat = form_Pat.GetPatient(out res);
            if (pat == null)
                MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                var Base = new cDataBase();
              //  if (Base.AddPatient(pat))
                    MessageBox.Show("Good");
               // else MessageBox.Show("Такой пациент уже существует в базе");
            }
        }

        private void text_ID_GotFocus(object sender, RoutedEventArgs e)
        {
            if (((TextBox) sender).Text.Equals("Введите ID"))
            {
                ((TextBox) sender).Text = "";
                ((TextBox) sender).Foreground = new SolidColorBrush((Color) ((TextBox) sender).Resources["TextColorBlack"]);
            }
        }

        private void text_ID_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text.Equals(""))
            {
                ((TextBox)sender).Text = "Введите ID";
                ((TextBox)sender).Foreground = new SolidColorBrush((Color)((TextBox)sender).Resources["TextColorGray"]);
            }
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


        private void but_find_Click(object sender, RoutedEventArgs e)
        {
            long id;
            if (Int64.TryParse(text_ID.Text, out id))
            {
               PatientEvent.Invoke(this,new PatientModelResponseArg{Action = BaseAction.Find,ID = id});
               AnamnesisEvent.Invoke(this,new AnamnesisModelResponseArg(){Action = BaseAction.Find});
            }
            else MessageBox.Show("Значение ID введено неверно", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void but_Add_Click(object sender, RoutedEventArgs e)
        {
            string res;
            text_ID.Text = "";
            text_ID_LostFocus(text_ID, null);
            var pat = form_Pat.GetPatient(out res);
            if (pat == null)
                MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                if(PatientEvent!=null)
                    PatientEvent.Invoke(this, new PatientModelResponseArg() { Action = BaseAction.AddPatient, Patient = pat });
            
        }

        private void text_ID_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                but_find_Click(this,null);
        }

        private void grid_Anam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var RV =  grid_Anam.SelectedValue as DataRowView;
            if (RV != null)
            {
                var bin_format = new BinaryFormatter();
                var mem_stream = new MemoryStream();
                mem_stream.Write((byte[]) RV.Row["Entries"], 0, ((byte[]) RV.Row["Entries"]).Count());
                mem_stream.Position = 0;
                DeviceDataEntry d = (DeviceDataEntry) bin_format.Deserialize(mem_stream);
                UpdateDataInGridRes(d);
                text_Info.Text = (string) RV.Row["Info"];
                AnalyzerEvent.Invoke(this,
               new AnalyzerCmdResponseArg()
               {
                   Cmd = AnalyzerCmd.SetTenzos,
                   DevDatEntry = d,
                   FltType = FilterType.MovingAverage,
                   FltParams = new[] { 2 }
               });
            }
        }

        public void UpdateDataInGridRes(DeviceDataEntry d)
       {
           //grid_Result.ItemsSource = d.GetCollection();
           Dispatcher.BeginInvoke(new Action(() => grid_Result.ItemsSource = d.GetCollection()));
           DrawGraph(d);
       }

        private void DrawGraph(DeviceDataEntry d)
        {
            var crvList = testGraph.GraphPane.CurveList;
            crvList.ForEach(item => item.Clear());
            for (int i = 0,j=0; i < d.AdcList.Count(); i++,j+=50)
            {
                var entry = d.AdcList[i];
                double time = j/1000.0;
                crvList[0].AddPoint(time, entry[0]);
                crvList[1].AddPoint(time, entry[1]);
                crvList[2].AddPoint(time, entry[2]);
                crvList[3].AddPoint(time, entry[3]);
            }
            // Вызываем метод AxisChange (), чтобы обновить данные об осях. 
            // В противном случае на рисунке будет показана только часть графика, 
            // которая умещается в интервалы по осям, установленные по умолчанию
            testGraph.AxisChange();

            // Обновляем график
            testGraph.Invalidate();
        }

        private void DrawGraph()
        {
            // Получим панель для рисования
            GraphPane pane = testGraph.GraphPane;


            pane.Chart.Fill.Brush = new SolidBrush(System.Drawing.Color.LightGray);
            pane.Fill.Color = System.Drawing.Color.LightGray;
            // Очистим список кривых на тот случай, если до этого сигналы уже были нарисованы
            pane.CurveList.Clear();
            // Создадим список точек
            PointPairList list = new PointPairList();
            
            double xmin = -50;
            double xmax = 50;

            // Заполняем список точек
            for (double x = xmin; x <= xmax; x += 0.01)
            {
                // добавим в список точку
                list.Add(x, f(x));
            }

            // Создадим кривую с названием "Sinc", 
            // которая будет рисоваться голубым цветом (Color.Blue),
            // Опорные точки выделяться не будут (SymbolType.None)
            LineItem myCurve = pane.AddCurve("Sinc", list, System.Drawing.Color.Blue, SymbolType.None);
            // Вызываем метод AxisChange (), чтобы обновить данные об осях. 
            // В противном случае на рисунке будет показана только часть графика, 
            // которая умещается в интервалы по осям, установленные по умолчанию
            testGraph.AxisChange();

            // Обновляем график
            testGraph.Invalidate();
        }

        private double f(double x)
        {
            if (x == 0)
            {
                return 1;
            }

            return Math.Sin(x) / x;
        }

        private void but_rec_Click(object sender, RoutedEventArgs e)
        {
            if (isStarted)
            {
                but_del_Click(this,null);
                but_rec.Visibility = Visibility.Collapsed;
                but_stop.Visibility = Visibility.Visible;
                var time = (int) slider.Value*1000;
                bar_meas.Maximum = time / MainConfig.ExchangeConfig.Period;
                if (DeviceCmdEvent != null)
                    DeviceCmdEvent.Invoke(this, new DeviceCmdArgEvent() { cmd = DeviceCmd.START_RECORDING,  MeasureTime = time});
            }
            else
            {
                MessageBox.Show("Необходимо запустить процесс замера перед началом записи", "Устройство не меряет",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void but_stop_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                but_stop.Visibility = Visibility.Collapsed;
                but_rec.Visibility = Visibility.Visible;
            }));
        }

        private void slider_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ((Slider)sender).Value += e.Delta > 0 ? 1 : -1;
        }

        private void but_st_Click(object sender, RoutedEventArgs e)
        {
            var  c = !isStarted ? DeviceCmd.START_MEASURE : DeviceCmd.STOP_MEASURE;

            isStarted ^= true;

            if (DeviceCmdEvent != null)
                DeviceCmdEvent.Invoke(this, new DeviceCmdArgEvent() {cmd = c});
            
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            testGraph.GraphPane.XAxis.Scale.Max = e.NewValue;
            testGraph.AxisChange();
            testGraph.Invalidate();
        }

        private void but_save_Click(object sender, RoutedEventArgs e)
        {
            double w = 0.0;
            var res = form_Pat.GetWeight(out w);
            if (res == "OK")
            {
                object[] d = new object[] {text_Info.Text, w};
                AnamnesisEvent.Invoke(this,
                    new AnamnesisModelResponseArg() 
                    {
                        Action = BaseAction.AddNewEntry,
                        EntryObjects = d
                    });
            }
            else
                MessageBox.Show(res, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void but_del_Click(object sender, RoutedEventArgs e)
        {
            AnamnesisEvent.Invoke(this,
                    new AnamnesisModelResponseArg()
                    {
                        Action = BaseAction.ClearEntry,
                        EntryState = BaseEntryState.New
                    });
        }

        private void but_setflt_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FilterDlgBox {Owner = this};
            dlg.ShowDialog();

            if(dlg.DialogResult != null && dlg.DialogResult.Value)
                AnalyzerEvent.Invoke(this,
                    new AnalyzerCmdResponseArg()
                    {
                     Cmd = AnalyzerCmd.ApplyFilter,
                     FltType = dlg.FlType,
                     FltParams = new[] {combo_GraphType.SelectedIndex, dlg.WindowFlt}
                    });
        }

        private void but_resetflt_Click(object sender, RoutedEventArgs e)
        {
            AnalyzerEvent.Invoke(this,
                new AnalyzerCmdResponseArg()
                {
                    Cmd = AnalyzerCmd.ResetAll,
                    FltParams = new[] { combo_GraphType.SelectedIndex }
                });
        }

        private void combo_GraphType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = ((ComboBox) sender);
            var graphtype = (GraphTypes) combo.SelectedIndex;

        }
    
     }

}

