using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Collections;
using System.Linq;
using System.Management;
using System.Threading;

namespace Stability.Model.Port
{
    public class  Pack
    {
        public List<byte> Data { get; set; }
        public Pack(List<byte> d)
        {
            Data = new List<byte>(d);
        }
   }

    public class CComPort : IPort
    {
        private readonly SerialPort _port;
    
        private List<byte> _rxBuf; 

        private Thread _rxThread;

        public Queue<Pack> RxData { get;private set; }
        public event EventHandler rx_event;

        private CComPort()
        {
            AutoConnect = false;
            _rxBuf = new List<byte>();
            RxData = new Queue<Pack>();
 
            _rxThread = new Thread(RxThreadHandler){IsBackground = true, Priority = ThreadPriority.Normal};
        }

        public CComPort(string portName, int baud = 9600):this()
        {
            _port = new SerialPort(portName,baud);
            _port.DataReceived+=PortOnDataReceived;
        }

        public CComPort(CPortConfig config):this(config.PortName,config.Baud)
        {
            string s;
            AutoConnect = config.AutoConnect;
            if (AutoConnect)
                Connect(out s);
        }

        public bool Connect(out string msg)
        {
            try
            {
                _port.Open();
                _rxThread.Start();
                msg = "OK";
                return true;
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
        }

        public bool Disconnect()
        {
            throw new System.NotImplementedException();
        }

        private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            var buf = new byte[_port.BytesToRead];
            _port.Read(buf, 0, buf.Length);
            
            _rxBuf.AddRange(buf);
            //buf.ToList().ForEach(b =>_rxBuf.Enqueue(b));    
      
        }

        private void RxThreadHandler()
        {
            var r = new List<byte>();

            while (true)
            {
                if (_rxBuf.Count > 0)
                {
                    var p_st = _rxBuf.FindIndex(0, o => o == 0xC0); //SLIP protocol
                    var p_end = _rxBuf.FindIndex(p_st+1, o => o == 0xC0);
                    if((p_st==-1)||(p_end==-1))
                        continue;
                    r = _rxBuf.GetRange(p_st+1, p_end - p_st-1);
                    _rxBuf.RemoveRange(p_st, p_end - p_st+1);

                   for (int i = 0; i < r.Count-1; i++)
                    {
                        if(r[i] == 0xDB)
                            if (r[i + 1] == 0xDC)
                            {
                                r.RemoveAt(i+1);
                                r[i] = 0xC0;
                            }
                            else if (r[i + 1] == 0xDD)
                                r.RemoveAt(i + 1);   
                    }
                    
                   /*var i = r.FindIndex(0, o => o == 0xDB);
                    if(i>-1)
                      if(r[i+1]==0xDC)
                        {
                            r[i] = 0xC0;
                            r.RemoveAt(i+1);
                        }
                      else if(r[i+1]==0xDC)
                         r.RemoveAt(i + 1);
                          
                    */
                   
                    RxData.Enqueue(new Pack(r));
                    if (rx_event != null)
                        rx_event.Invoke(this,null);
                    r.Clear();
             }

                Thread.Sleep(20);
            }
        }


        public static bool FindPort(string caption, out string portName)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2",
                        "SELECT * FROM Win32_PnPEntity");
            portName = "";
            var mng = searcher.Get();
            var l = new List<string>();
            foreach (ManagementObject obj in mng)
            {
                object captionObj = obj["Caption"];
                if (captionObj != null)
                {
                    string cap = captionObj.ToString();
                    if (cap.Contains(caption))
                        l.Add(cap);
                }
                else return false;
            }

            
            var pnl = SerialPort.GetPortNames();

            if (!pnl.Any()) return false;

            portName = pnl.ToList().Find(s => l[0].Contains(s));

            /*if (pnl.Count() > 0)
            {
                foreach (var n in pnl)
                {
                    if (l[0].Contains(n))
                        portName = n;
                }

               
            }*/
            return true;
        }

        public void Test(byte c)
        {
            var cmd = new byte[] {0xC0, c, 0xC0};
            _port.Write(cmd, 0, cmd.Length);
        }


        public bool AutoConnect { get; set; }
       
    }
}
