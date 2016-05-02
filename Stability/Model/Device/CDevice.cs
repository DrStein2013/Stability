using System;
using Stability.Model.Port;
using Stability.View;

namespace Stability.Model.Device
{
    class CDevice
    {
        //protected Queue<Pack> RxData;
        protected IPort Port { get; private set; }
        public EventHandler MeasurementsDone; 

        public CDevice()
        {
            //RxData = new Queue<Pack>();
            Port = IoC.Resolve<IPort>();
        }

        public virtual void Calibrate(CalibrationParams calibParams, params object[] pObjects)
        {
            throw new NotImplementedException();
        }

        protected virtual void Parse(Pack p)
        {
            throw new NotImplementedException();
        }

        protected void SendCmd(byte[] cmd)
        {
            Port.SendData(cmd);
        }
    }
}
