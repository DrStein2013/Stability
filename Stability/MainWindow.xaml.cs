using System;
using System.Windows;
using System.Windows.Media;
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
            //var n = CComPort.FindPort("Stabilometric Device"/*"USB Serial Port"*/, out name);

            // var conf = MainConfig.PortConfig;
            // var conf = new CPortConfig() {PortName = "COM9",/*name*/ AutoConnect = true, Baud = 9600, UseSLIP = true};
            /*c = new CComPort(conf);
                c.RxEvent += COnRxEvent;
                c.PortStatusChanged += COnPortStatusChanged;
            */

            /*     c =  IoC.Resolve<CComPort>(new ConstructorArgument("config", conf));
               c.RxEvent += COnRxEvent;
               c.PortStatusChanged += COnPortStatusChanged;*/
            //   MainConfig.Load();
            //IoC.GetKernel().Bind<IPort>().To<CComPort>().InSingletonScope().WithConstructorArgument("config", conf);

            //c = IoC.Resolve<CComPort>(new ConstructorArgument("config", conf));
            //  c = (CComPort) IoC.Resolve<IPort>();
            //   c.RxEvent += COnRxEvent;
            //   c.PortStatusChanged += COnPortStatusChanged;


            //   st = new StabilityDevice();
        }

        public void COnPortStatusChanged(object sender, PortStatusChangedEventArgs portStatusChangedEventArgs)
        {
            if(portStatusChangedEventArgs.Status == EPortStatus.Open)
                Dispatcher.BeginInvoke(new Action(() => StatusMark.Fill = new SolidColorBrush(Colors.Green)));
            else if(portStatusChangedEventArgs.Status == EPortStatus.Closed)
                Dispatcher.BeginInvoke(new Action(() => StatusMark.Fill = new SolidColorBrush(Colors.Red)));
        }

        private void COnRxEvent(object sender, EventArgs eventArgs)
        {
            string s = "";
          /*  var dat = c.RxData.Dequeue().Data;
            foreach (var b in dat)
            {
             //   s += b.ToString("X2")+" ";
            }
          Dispatcher.BeginInvoke(new Action(() => RxTextBlock.Text += s)); */
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_OnChecked(object sender, RoutedEventArgs e)
        {
            Con.Visibility = Visibility.Collapsed;
            Discon.Visibility = Visibility.Collapsed;
          //  ViewUpdated.Invoke(this,null);
           // if(c!=null)
           //     c.AutoConnect = true;
        }

        private void MenuItem_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Con.Visibility = Visibility.Visible;
            Discon.Visibility = Visibility.Visible;
            ViewUpdated.Invoke(this, null);
          //  if (c != null)
          //      c.AutoConnect = false;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
        /*   var sym = TxEdit.Text.ToCharArray();
            var buf = new byte[sym.Length];
            for (int i = 0; i < buf.Length; i++)
                buf[i] = Convert.ToByte(sym[i]);
    */
          //  c.SendData(buf);
            
          //  st.StartMeasurement();
        }

        private void Discon_OnClick(object sender, RoutedEventArgs e)
        {
           // c.Disconnect();
        }
        
        private void Con_OnClick(object sender, RoutedEventArgs e)
        {
            string s;
           // if (!c.Connect(out s))
            //    MessageBox.Show(this, s, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void UpdateView()
        {
            throw new NotImplementedException();
        }

        public event EventHandler ViewUpdated;
    }
}
