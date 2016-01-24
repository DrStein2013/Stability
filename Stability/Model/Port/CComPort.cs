using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using Ninject;

namespace Stability.Model.Port
{
    public class  Pack
    {
        public List<byte> Data { get; set; }
        public Pack(IEnumerable<byte> d)
        {
            Data = new List<byte>(d);
        }
   }

    public class CComPort : IPort
    {
        private readonly SerialPort _port;
    
        private readonly List<byte> _rxBuf;

        private readonly ManualResetEvent _rxManualResetEvent = new ManualResetEvent(false);

        private readonly Timer _statusUpdaterTimer;
        /// <summary>
        /// Пороговое количество пакетов для вычитки
        /// </summary>
        private byte _threshold = 0;
        private bool _useSLIP;

        /// <summary>
        /// Очередь, где накапливаются уже вычитанные и обработанные пакеты.
        /// </summary>
        public Queue<Pack> RxData { get;private set; }
        /// <summary>
        /// Указывает, необходимость автоматического восстановления связи
        /// </summary>
        public bool AutoConnect { get; set; }

        /// <summary>
        /// Указывает необходимость использования SLIP протокола
        /// </summary>
        public bool UseSLIP
        {
            get { return _useSLIP; }
            set
            {
                _useSLIP = value;
                _threshold = (byte) (!_useSLIP ? 1 : 0);    //Если SLIP не используется - порог реакции - 10 входящих байт, иначе реакция без ожидания 
            }
        }

        public EPortStatus Status { get;private set; }

        /// <summary>
        /// Событие вычитки из порта, взводится, когда есть что читать.
        /// Привязать на это событие обработчик вычитки данных
        /// </summary>
        /// <remarks></remarks>
        public event EventHandler RxEvent;

        /// <summary>
        /// Событие изменения статуса порта, сигналит открыт порт, или нет
        /// </summary>
        public event EventHandler<PortStatusChangedEventArgs> PortStatusChanged; 

        private CComPort()
        {
            AutoConnect = false;
            UseSLIP = false;
            _rxBuf = new List<byte>();
            RxData = new Queue<Pack>();
            Status = EPortStatus.Closed;
            _statusUpdaterTimer = new Timer(StatusTimerHandler, null,Timeout.Infinite, 2000);      
            var rxThread = new Thread(RxThreadHandler){IsBackground = true, Priority = ThreadPriority.Normal};
            rxThread.Start(_rxManualResetEvent);
        }

        
        public CComPort(string portName, int baud = 9600):this()
        {
            _port = new SerialPort(portName,baud);
            _port.DataReceived+=PortOnDataReceived;
            _statusUpdaterTimer.Change(0, 2000);
        }

        public CComPort(CPortConfig config):this(config.PortName,config.Baud)
        {
            string s;
            UseSLIP = config.UseSLIP;
            AutoConnect = config.AutoConnect;
           // if (AutoConnect)
           //     Connect(out s);
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
                    Status = EPortStatus.Open;
                    _rxManualResetEvent.Set();
                }
                msg = "OK";
                return true;
            }
            catch (Exception e)
            {
                Status = EPortStatus.Closed;
                msg = e.Message;
                return false;
            }
        }

        /// <summary>
        /// Метод разрыва соединения и остановки потока вычитки
        /// </summary>
        public bool Disconnect()
        {
            if (_port.IsOpen)
            {
                _port.Close();
                Status = EPortStatus.Closed;
            }
           _rxManualResetEvent.Reset();
            RxData.Clear();
            _rxBuf.Clear();
            return true;
        }

        private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            var buf = new byte[_port.BytesToRead];
            _port.Read(buf, 0, buf.Length);
            
            _rxBuf.AddRange(buf);
        }

        private void RxThreadHandler(object ev)
        {
           var stopEvent = (ManualResetEvent)ev;
           while (stopEvent.WaitOne())
            {              
             if (_rxBuf.Count > 0)       //Если в буфере есть данные
             {
                 Pack pack;
                 if (UseSLIP)
                     pack = SlipParser(); // то прогоняем их через SLIP протокол
                 else
                 {
                     pack = new Pack(_rxBuf.GetRange(0,_threshold));
                     _rxBuf.RemoveRange(0,_threshold);
                 }

                  if (pack != null)
                   {
                    RxData.Enqueue(pack);   //Суем пакет в выходную очередь
                       if (RxEvent != null)
                          RxEvent.Invoke(this, null); //Дергаем Event   
                           
                           
                   }
                }
                
               Thread.Sleep(20);
            }
        }

        private Pack SlipParser()
        {
            var pSt = _rxBuf.FindIndex(0, o => o == 0xC0); //Ищем первое вхождение токена 0xC0 - это начало пакета
            var pEnd = _rxBuf.FindIndex(pSt + 1, o => o == 0xC0); //Ищем следующий 0xC0 - это конец пакета
            if ((pSt == -1) || (pEnd == -1))      //Если кого-то не нашли, то данные в буфер поступить не успели и надо еще подождать
              return null;

            var r = _rxBuf.GetRange(pSt + 1, pEnd - pSt - 1);    //Выделяем обнаруженный пакет из списка
            _rxBuf.RemoveRange(pSt, pEnd - pSt + 1); //Удаляем пакет из списка (получается аналог очереди через List)

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
        
            return new Pack(r);
        }

        /// <summary>
        /// Выполняет поиск порта в реестре устройств компьютера по заданному названию
        /// </summary>
        public static bool FindPort(string caption, out string portName)   
        {
            var searcher = new ManagementObjectSearcher("root\\CIMV2", //Формируем WMI запрос для получения списка железа
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

        /// <summary>
        /// Выполняет отправку данных по интрефейсу RS232
        /// </summary>
        public void SendData(byte[] b)
        {
            var buf = b;
            if(_useSLIP)
                buf = ToSlipBytes(b);

            try
            {
                _port.Write(buf, 0, buf.Length);
            }
            catch (Exception e)
            {
                return;
            }
        }

        public Queue<Pack> GetRxBuf()
        {
            return RxData;
        }

        /// <summary>
        /// Выполняет отправку данных по интрефейсу RS232
        /// </summary>
        public void SendData(Pack p)
        {
            SendData(p.Data.ToArray());
        }

        private byte[] ToSlipBytes(IEnumerable<byte> b)
        {
            var list = b.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == 0xC0)
                {
                    list[i] = 0xDB;
                    list.Insert(++i,0xDC);
                } else if(list[i] == 0xDB)
                    list.Insert(++i,0xDD);
            }
            list.Insert(0,0xC0);
            list.Insert(list.Count,0xC0);
            return list.ToArray();
        }

        private void StatusTimerHandler(object state)
        {
            if (!_port.IsOpen) //Если порт оказался закрытым 
            {
              Status = EPortStatus.Closed; //Меняем статус
              if(AutoConnect) //и надо его открывать автоматически
              {
                  string s;
                  Connect(out s); //пробуем открыть
              }
            }

            if (PortStatusChanged != null)  //Если на событие статуса кто-то подписался, то вызываем обработчик, передав ему туда текущий статус
                PortStatusChanged.BeginInvoke(this, new PortStatusChangedEventArgs { Status = Status }, null, null);
        }

    }
}
