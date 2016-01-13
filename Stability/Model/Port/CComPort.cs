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
    
        private readonly List<byte> _rxBuf; 

        private readonly Thread _rxThread;

        public Queue<Pack> RxData { get;private set; }
        public bool AutoConnect { get; set; }

        /// <summary>
        /// Событие вычитки из порта, взводится, когда есть что читать.
        /// Привязать на это событие обработчик вычитки данных
        /// </summary>
        /// <remarks></remarks>
        public event EventHandler RxEvent;

        /// <summary>
        /// Событие изменения статуса порта, показывает открыт порт, или нет
        /// </summary>
        public event EventHandler<PortStatusChangedEventArgs> PortStatusChanged; 

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
            _port.PinChanged += PortOnPinChanged;
        }

        public CComPort(CPortConfig config):this(config.PortName,config.Baud)
        {
            string s;
            AutoConnect = config.AutoConnect;
            if (AutoConnect)
                Connect(out s);
        }

        /// <summary>
        /// Метод открытия порта RS-232.
        /// </summary>
        /// <returns>true - если порт успешно открыт; false - если порт открыть не удалось, тогда msg содержит описание ошибки</returns>
        public bool Connect(out string msg)
        {
            try
            {
                if (!_port.IsOpen)
                {
                    _port.Open();
                    if(!_rxThread.IsAlive)
                        _rxThread.Start();
                }
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
            if(_port.IsOpen)
                _port.Close();
            if(_rxThread.IsAlive) 
                _rxThread.Abort();
            RxData.Clear();
            _rxBuf.Clear();
            return true;
        }

        private void PortOnPinChanged(object sender, SerialPinChangedEventArgs serialPinChangedEventArgs)
        {
           if(serialPinChangedEventArgs.EventType == SerialPinChange.Break)
              PortStatusChanged.Invoke(this,new PortStatusChangedEventArgs(){Status = (EPortStatus)Convert.ToInt16(_port.IsOpen)});
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
            while (true)
            {
                    if (_rxBuf.Count > 0)       //Если в буфере есть данные, то прогоняем их через SLIP протокол
                    {
                        var p_st = _rxBuf.FindIndex(0, o => o == 0xC0); //Ищем первое вхождение токена 0xC0 - это начало пакета
                        var p_end = _rxBuf.FindIndex(p_st + 1, o => o == 0xC0); //Ищем следующий 0xC0 - это конец пакета
                        if ((p_st == -1) || (p_end == -1))      //Если кого-то не нашли, то данные в буфер поступить не успели и надо еще подождать
                            continue;

                        var r = _rxBuf.GetRange(p_st + 1, p_end - p_st - 1);    //Выделяем обнаруженный пакет из списка
                        _rxBuf.RemoveRange(p_st, p_end - p_st + 1); //Удаляем пакет из списка (получается аналог очереди через List)

                        for (int i = 0; i < r.Count - 1; i++)   //Ищем токен 0xDB, т.к. он указывает на подмену символа 
                        {
                            if (r[i] == 0xDB)   
                                if (r[i + 1] == 0xDC)   //Если имеем 0xDB,0xDC, то это заменитель 0xC0
                                {
                                    r.RemoveAt(i + 1);  //Удаляем один
                                    r[i] = 0xC0;        //Заменяем второй
                                }
                                else if (r[i + 1] == 0xDD)  //Если имеем 0xDB,0xDC, то это заменитель 0xDB
                                    r.RemoveAt(i + 1);  //Просто удаляем лишний
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

                        RxData.Enqueue(new Pack(r));    //Суем пакет в выходную очередь
                        if (RxEvent != null)            
                            RxEvent.Invoke(this, null); //Дергаем Event
                        r.Clear();
                    }
                
               if((!_port.IsOpen)&&(AutoConnect))   //Если порт оказался закрытым и надо его открывать автоматически
               {
                    string s;
                    if(!Connect(out s))             //пробуем открыть
                        Thread.Sleep(980);          //если не открылось спим около секунды
               }
               Thread.Sleep(20);
            }
        }

        public static bool FindPort(string caption, out string portName)   
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", //Формируем WMI запрос для получения списка железа
                        "SELECT * FROM Win32_PnPEntity");
            portName = "";              
            var mng = searcher.Get();       //получаем список
            var l = new List<string>();
            foreach (ManagementObject obj in mng)   //перебором просматриваем все заголовки в поиске нужного заголовка
            {
                object captionObj = obj["Caption"];
                if (captionObj != null)
                {
                    string cap = captionObj.ToString();
                    if (cap.Contains(caption))
                        l.Add(cap);                 //найденный заголовок вносим в список
                }
                else return false;
            }

            
            var pnl = SerialPort.GetPortNames();    //получаем имена СОМ портов в системе

            if ((!pnl.Any())||(!l.Any())) return false; //Если нет портов вообще, или нет нужного нам, то уходим
                        
            portName = pnl.ToList().Find(s => l[0].Contains(s));    //Если есть, то отделяем котлетки от мух:) и получаем имя нужного нам порта

            /*if (pnl.Count() > 0)
            {
                foreach (var n in pnl)
                {
                    if (l[0].Contains(n))
                        portName = n;
                }

               
            }*/
            return true;    //Все нашли:)
        }

        public void Test(byte c)
        {
            var cmd = new byte[] {0xC0, c, 0xC0};
            _port.Write(cmd, 0, cmd.Length);
        }
       
    }
}
