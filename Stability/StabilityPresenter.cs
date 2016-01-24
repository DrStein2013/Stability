using System;
using Stability.Model;
using Stability.Model.Port;
using Stability.View;

namespace Stability
{
    class StabilityPresenter
    {
        private IStabilityModel _model;
        private IView _view;

        public StabilityPresenter(IStabilityModel model,IView view)
        {
            _model = model;
            _model.UpdateDataView += ModelOnUpdateDataView;


            _view = view;
            _view.ViewUpdated += ViewOnViewUpdated;
            _view.DeviceCmdEvent += ViewOnDeviceCmdEvent;
            IoC.Resolve<IPort>().PortStatusChanged += _view.COnPortStatusChanged;
        }

        private void ViewOnDeviceCmdEvent(object sender, DeviceCmdArgEvent deviceCmdArgEvent)
        {
           _model.DeviceCmdFromView(deviceCmdArgEvent.cmd);
        }

        private void ModelOnUpdateDataView(object sender, TenzEventArgs tenzEventArgs)
        {
            _view.UpdateTenzView(new[]
            {
                tenzEventArgs.Data[0].ToString("F2"),
                tenzEventArgs.Data[1].ToString("F2"),
                tenzEventArgs.Data[2].ToString("F2"),
                tenzEventArgs.Data[3].ToString("F2")
            });
        }

        private void ViewOnViewUpdated(object sender, EventArgs eventArgs)
        {
            GetViewState();
        }

        private void GetViewState()
        {
            
        }
    }
}
