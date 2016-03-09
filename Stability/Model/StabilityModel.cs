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

    public interface IStabilityModel
    {
        event EventHandler<TenzEventArgs> UpdateDataView;
        event EventHandler<TenzEventArgs> UpdateWeightKoef;
        void DeviceCmdFromView(DeviceCmdArgEvent c);
    }

    public class StabilityModel : IStabilityModel
    {
        private StabilityDevice _device;
        public event EventHandler<TenzEventArgs> UpdateDataView;
        public event EventHandler<TenzEventArgs> UpdateWeightKoef;
        public bool ShowAdcs { get; set; }
        private readonly Timer _viewUpdaterTimer;

        public StabilityModel()
        {
            var conf = MainConfig.PortConfig;
            IoC.GetKernel().Bind<IPort>().To<CComPort>().InSingletonScope().WithConstructorArgument("config", conf);
            _device = new StabilityDevice();
            
            _device.calibrationDone +=
                (sender, args) => UpdateWeightKoef(this, new TenzEventArgs() {Data = _device._weighKoefs});

            _viewUpdaterTimer = new Timer(ViewTimerHandler, null,100, 60);
        }

        private void ViewTimerHandler(object state)
        {
            var d = new double[4];
            d = ShowAdcs ? _device.CurrAdcVals : _device.WeightDoubles;

            if (UpdateDataView != null) 
                UpdateDataView(this, new TenzEventArgs() { Data = d});
        }

        public void DeviceCmdFromView(DeviceCmdArgEvent deviceCmdArgEvent)
        {
            switch (deviceCmdArgEvent.cmd)
            {
                    case DeviceCmd.START_MEASURE:
                        _device.StartMeasurement();
                    break;
                    case DeviceCmd.STOP_MEASURE:
                        _device.StopMeasurement();
                    break;
                    case DeviceCmd.ZERO_CALIBRATE:
                        _device.Calibrate(null);
                    break;
                    case DeviceCmd.WIEGHT_CALIBRATE:
                        _device.Calibrate(deviceCmdArgEvent.Params);
                    break;
                default:
                    break;
            }
        }
    }
}
