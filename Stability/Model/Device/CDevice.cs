using System;
using Stability.Model.Port;

namespace Stability.Model.Device
{
    class CDevice
    {
        //protected Queue<Pack> RxData;
        protected IPort Port { get; private set; }

        public CDevice()
        {
            //RxData = new Queue<Pack>();
            Port = IoC.Resolve<IPort>();
        }

        protected virtual void Calibrate()
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
