using System;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using Stability.Model.Port;

namespace Stability.Model
{
    public class CDevice : IDevice
    {
        private IPort _port;

        CDevice()
        {
            
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
