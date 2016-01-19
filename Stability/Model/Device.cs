using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using Stability.Model.Port;

namespace Stability.Model
{
    public abstract class CDevice
    {
        private IPort _port;
        private Queue<Pack> RxData;

        CDevice()
        {
            _port.RxEvent+=PortOnRxEvent;
        }

        private void PortOnRxEvent(object sender, EventArgs eventArgs)
        {
            var p = _port as CComPort;
           RxData.Enqueue(p.RxData.Dequeue());

        }

        public void Calibrate()
        {
            throw new NotImplementedException();
        }

        public void StartMeasurement()
        {
            throw new NotImplementedException();
        }

        public void StopMeasurement()
        {
            throw new NotImplementedException();
        }
    }
}
