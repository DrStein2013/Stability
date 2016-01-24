using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            IoC.Resolve<IPort>().PortStatusChanged += _view.COnPortStatusChanged;

        }

        private void ViewOnViewUpdated(object sender, EventArgs eventArgs)
        {
            GetViewState();

        }

        private void ModelOnUpdateDataView(object sender, EventArgs eventArgs)
        {
           
        }

        private void GetViewState()
        {
            
        }
    }
}
