using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stability.Model.Device;
using Stability.Model.Port;
using System.Threading;
using Stability.View;


namespace Stability.Model
{

    public class TenzEventArgs : EventArgs
    {
        public double[] Data { get; set; }
    }

    interface IStabilityModel
    {
        event EventHandler<TenzEventArgs> UpdateDataView;
        void DeviceCmdFromView(DeviceCmd c);
    }

    public class StabilityModel : IStabilityModel
    {
        private StabilityDevice _device;
        public event EventHandler<TenzEventArgs> UpdateDataView;

        private readonly Timer _viewUpdaterTimer;

        public StabilityModel()
        {
            var conf = MainConfig.PortConfig;
            IoC.GetKernel().Bind<IPort>().To<CComPort>().InSingletonScope().WithConstructorArgument("config", conf);
            _device = new StabilityDevice();
            
            _viewUpdaterTimer = new Timer(ViewTimerHandler, null,100, 60);
        }

        private void ViewTimerHandler(object state)
        {
            if (UpdateDataView != null) 
                UpdateDataView(this, new TenzEventArgs() {Data = _device.CurrAdcVals});
        }

        public void DeviceCmdFromView(DeviceCmd c)
        {
            switch (c)
            {
                    case DeviceCmd.START_MEASURE:
                        _device.StartMeasurement();
                    break;
                    case DeviceCmd.STOP_MEASURE:
                        _device.StopMeasurement();
                    break;
                default:
                    break;
            }
        }
    }
}
