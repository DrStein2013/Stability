using System;
using System.Collections.Generic;
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
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace Stability
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IView
    {
        private StabilityPresenter _presenter;
        private ButtonHandler buttonHandler;
        public MainWindow()
        {
            InitializeComponent();
            _presenter = new StabilityPresenter(new StabilityModel(), this);
            
            Thread.Sleep(200);
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
        public event EventHandler<PatientModelResponseArg> PatientEvent;

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
       /*    var win = new SimpleWindow("Вес для калибровки","Введите значения веса для калибровки:");
    
           if((bool)win.ShowDialog())
             if (DeviceCmdEvent != null)
                DeviceCmdEvent.Invoke(this, new DeviceCmdArgEvent() { cmd = DeviceCmd.WEIGHT_CALIBRATE_FAST,Params = new CalibrationParams(){Weight = win.Value,EntryCount = 100}});*/
      
            /* var adp = new PatientBaseDataSetTableAdapters.PatientTableAdapter();
            TestBaseDataSet.PatientDataTable t = adp.GetData();
            var d = t.Select("Name = 'Lesha'");
            foreach (var pt in d)
            {
                var c = pt as TestBaseDataSet.PatientRow;
                c.Age++;
                var str = c.Name + " " + c.Age.ToString();
                MessageBox.Show(str);
                adp.Update(c);
            }*/
            /*var adp = new PatientBaseDataSetTableAdapters.NamesTableAdapter();

            var t = adp.GetDataBy("Леша");

            foreach (var id in t)
            {
                MessageBox.Show(id.ID + " " + id.Name);
            }
            
            var ad = new PatientBaseDataSetTableAdapters.PatientTableAdapter();//PatientTableAdapter();
         //   ad.Insert(t[0].ID, 0, 0, new DateTime(), false, 0, 0);*/

        /*-----------------------------------------------------------
            var adp = new PatientBaseDataSetTableAdapters.Pat_TabTableAdapter();
         
            try
            {
                var data = adp.GetData();//GetDataByID(1);
                grid.ItemsSource = data;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
            */

            var Base = new cDataBase();

            //grid.ItemsSource = Base.GetPatientBy(1);
            PatientBaseDataSet.Pat_TabDataTable t = null;
            var pat = Base.FindPatientBy(1, ref t);
            if (pat != null)
            {
                grid.ItemsSource = t;
                form_Pat.SetToForm(pat);
            }
            else
                MessageBox.Show("Пациент с таким ID отсутствует в базе", "Error", MessageBoxButton.OK,
                                MessageBoxImage.Error);
            /* string s = "ID = " + data[0].ID + " " + data[0].Имя + " " + data[0]["Фамилия"];
            MessageBox.Show(s);*/
        }

        private void WeightParamItem_OnClick(object sender, RoutedEventArgs e)
        {

            var pat = new cPatient()
            {
                Name = "Артур",
                Surname = "Бережной",
                Patronymic = "Борисович",
                Sex = true,
                Birthdate = new DateTime(1992, 7, 7),
                Height = 170,
                Address = new cAddress() { Street = "Островского", House = 20, Flat = 97 },
                PhoneNumber = "0632737032"
            };
            var Base = new cDataBase();
         //   Base.AddPatient(pat);

            MessageBox.Show("Good");
            /*var r = adp_name.GetByName(name);

            if (r.Count == 0)
            {
                adp_name.Insert(name);
                r = adp_name.GetByName(name);
                tab_patient[0].Name_ID = r[0].ID;
            }
            */



            // MessageBox.Show();
            /* adp.Insert("Миша");
            adp.Insert("Рома");
            adp.Insert("Леша");
            var r = adp.GetID();
           */

            /*  var t = adp.GetIDByName();
            
            var ad = new PatientBaseDataSetTableAdapters.PatientTableAdapter();//PatientTableAdapter();
            ad.Insert(t[0].ID, 0, 0, new DateTime(), false, 0, 0);*/
            //       adp.InsertQuery("Lesha", 20);


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
            var an = new cAnamnesisEntry() {Entry = t, Info = "", Weight = 59};
            Base.AddAnamnesis(an);
        }

        private void Test2_OnClick(object sender, RoutedEventArgs e)
        {
            var adp = new PatientBaseDataSetTableAdapters.AnamnesisTableAdapter();
            var t = adp.GetDataBy(1);

            var b_arr = t[0].Entries;

            var bin_format = new BinaryFormatter();
            var mem_stream = new MemoryStream();
            mem_stream.Write(b_arr,0,b_arr.Count());
            mem_stream.Position = 0;
            DeviceDataEntry d = (DeviceDataEntry) bin_format.Deserialize(mem_stream);
            d.AdcList.Add(new double[]{5,4,3,7});

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
                    PatientEvent.Invoke(this, new PatientModelResponseArg() { Action = BaseAction.Add, Patient = pat });
            
        }

        private void text_ID_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                but_find_Click(this,null);
        }

    
     }

}

