using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stability.Model.Device;
using Stability.Model.Port;

namespace Stability.Model
{
    interface IStabilityModel
    {
        event EventHandler UpdateDataView;
    }

    public class StabilityModel : IStabilityModel
    {
        private StabilityDevice _device;
        public event EventHandler UpdateDataView;

        public StabilityModel()
        {
            var conf = MainConfig.PortConfig;
            IoC.GetKernel().Bind<IPort>().To<CComPort>().InSingletonScope().WithConstructorArgument("config", conf);
            _device = new StabilityDevice();
        }

        public void SetPortHandler(EventHandler<PortStatusChangedEventArgs> pHandler)
        {
            IoC.Resolve<IPort>().PortStatusChanged += pHandler;
        }


    }
}
