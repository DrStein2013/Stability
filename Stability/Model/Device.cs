using System;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using Stability.Model.Port;

namespace Stability.Model
{
    public class CDevice : IDevice
    {
        private CDevice()
        {
            //_port = new SerialPort();
            AutoConnect = false;
           _rxthread = new Thread(RxThreadHandler){IsBackground = true,Priority = ThreadPriority.Normal};
           _rxBuf = new Queue();
        }

        public CDevice(string portName, int baud = 9600):this() 
        {
            _port = new SerialPort(portName,baud);
           /* AutoConnect = false;
            _rxthread = new Thread(RxThreadHandler) { IsBackground = true, Priority = ThreadPriority.Normal };
            _rxBuf = new Queue();*/
        }

        public CDevice(CPortConfig config)
        {
            _rxBuf = new Queue();
            _rxthread = new Thread(RxThreadHandler) { IsBackground = true, Priority = ThreadPriority.Normal };

            _port = new SerialPort(config.PortName,config.Baud);
            AutoConnect = config.AutoConnect;
            if (AutoConnect)
                Connect();
        }

        public SerialPort _Port
        {
            get { return _port; }
        }



        public bool Connect()
        {
            try
            {
                _port.Open();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Disconnect()
        {
            throw new System.NotImplementedException();
        }

     

        public void Calibrate()
        {
            throw new System.NotImplementedException();
        }

        public void StartMeasurement()
        {
            throw new System.NotImplementedException();
        }

        public void StopMeasurement()
        {
            throw new System.NotImplementedException();
        }

        //-----------Static Methods----------------------------------
        /// <summary>
        /// Поиск порта, к которому подключено устройство
        /// </summary>
        public static bool FindPort(out string portName)
        {
            throw new System.NotImplementedException();
        }

        private void RxThreadHandler()
        {
            throw new System.NotImplementedException();
        }

        public bool AutoConnect { get; set; }

        private Thread _rxthread;
        private SerialPort _port;
        private Queue _rxBuf;
    }
}
