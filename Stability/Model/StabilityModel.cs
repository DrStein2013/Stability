using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stability.Enums;
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
        event EventHandler<WeightEventArgs> UpdateWeight; 
        void DeviceCmdFromView(DeviceCmdArgEvent c);
        void SetNewConfig(CPortConfig c, StabilityExchangeConfig stabilityExchangeConfig);
    }

    public class StabilityModel : IStabilityModel
    {
        private readonly StabilityDevice _device;
        public event EventHandler<TenzEventArgs> UpdateDataView;
        public event EventHandler<TenzEventArgs> UpdateWeightKoef;
        public event EventHandler<WeightEventArgs> UpdateWeight; 
        public bool ShowAdcs { get; set; }
        private readonly Timer _viewUpdaterTimer;

        public StabilityModel()
        {
            var conf = MainConfig.PortConfig;
            IoC.GetKernel().Bind<IPort>().To<CComPort>().InSingletonScope().WithConstructorArgument("config", conf);
            _device = new StabilityDevice();
            
            _device.CalibrationDone +=
                (sender, args) =>
                {
                    if (UpdateWeightKoef != null)
                        UpdateWeightKoef(this, new TenzEventArgs() {Data = _device.WeightKoefs});
                };

            _device.WeightMeasured+=DeviceOnWeightMeasured;

            _viewUpdaterTimer = new Timer(ViewTimerHandler, null,100, 60);
        }

        private void DeviceOnWeightMeasured(object sender, WeightEventArgs weightEventArgs)
        {
            UpdateWeight(this, weightEventArgs);
        }

        private void ViewTimerHandler(object state)
        {
            double[] d;
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
                        _device.Calibrate(null,false,100);
                    break;
                    case DeviceCmd.STARTUP_CALIBRATE:
                        _device.Calibrate(null,true,100);
                    break;
                    case DeviceCmd.WEIGHT_CALIBRATE:
                        _device.Calibrate(deviceCmdArgEvent.Params,false);
                    break;
                    case DeviceCmd.WEIGHT_CALIBRATE_FAST:
                        _device.Calibrate(deviceCmdArgEvent.Params,true);
                    break;
                    case DeviceCmd.WEIGHT_MEASURE:
                        _device.GetWeight();
                    break;
                default:
                    break;
            }
        }

        public void SetNewConfig(CPortConfig c, StabilityExchangeConfig stabilityExchangeConfig)
        {
            var p = (CComPort) IoC.Resolve<IPort>();
            p.Reconfig(c);
            _device.ExchangeConfig = stabilityExchangeConfig;
        }

        public void SetNewKoefs(double[] w_koefs)
        {
            _device.WeightKoefs = w_koefs;
        }
    }
}
