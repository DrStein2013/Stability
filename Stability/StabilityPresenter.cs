using System;
using Stability.Model;
using Stability.Model.Port;
using Stability.View;

namespace Stability
{
    class Presenter
    {
        protected IStabilityModel _model;
        protected IView _view;

        public Presenter(IStabilityModel model, IView view)
        {
            _model = model;
            _view = view;
            _view.ViewUpdated += ViewOnViewUpdated;
            _view.DeviceCmdEvent += ViewOnDeviceCmdEvent;
        }

        private void ViewOnDeviceCmdEvent(object sender, DeviceCmdArgEvent e)
        {
            _model.DeviceCmdFromView(e);
        }

        private void ViewOnViewUpdated(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }


    class StabilityPresenter : Presenter
    {
        public IStabilityModel Model { get { return _model; } }

        public StabilityPresenter(IStabilityModel model,IView view):base(model,view)
        {
            _model.UpdateDataView += ModelOnUpdateDataView;
            IoC.Resolve<IPort>().PortStatusChanged += _view.COnPortStatusChanged;
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

    class CalibratePresenter : Presenter
    {
        public CalibratePresenter(IStabilityModel model, IView view) : base(model, view)
        {
            model.UpdateDataView+=ModelOnUpdateDataView;
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
    }
}
