using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using Ninject.Parameters;
using Stability.Model.Port;

namespace Stability.Model
{
    class cDevice
    {
        private IPort _port;
        private Queue<Pack> RxData;

        public cDevice()
        {
            var c = IoC.Resolve<IPort>();
            c.SendData(new byte[]{0x31});
            //c.Disconnect();
            /*_port*/
        }

        private static void PortOnRxEvent(object sender, EventArgs eventArgs)
        {
           
           //RxData.Enqueue();

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
