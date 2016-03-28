using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Stability.Model;
using Stability.Model.Device;
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
            model.UpdateWeightKoef+=ModelOnUpdateWeightView;
            _view.ViewUpdated += ViewOnViewUpdated;
            CurrWeightKoefs = new double[4];

            ((Window)_view).Closing += (sender, args) =>
            {
                ((StabilityModel) _model).SetNewKoefs(MainConfig.WeightKoefs);
                _model.UpdateWeightKoef -= ModelOnUpdateWeightView;
            };
        }

        private void ViewOnViewUpdated(object sender, EventArgs eventArgs)
        {
            CurrWeightKoefs = ((CalibrationWindow) _view).GetWeightDoubles();
            ((StabilityModel)_model).SetNewKoefs(CurrWeightKoefs);
        }

        private void ModelOnUpdateWeightView(object sender, TenzEventArgs tenzEventArgs)
        {
            CurrWeightKoefs = tenzEventArgs.Data.ToArray();
            _view.UpdateTenzView(new[]
            {
                tenzEventArgs.Data[0].ToString("F2"),
                tenzEventArgs.Data[1].ToString("F2"),
                tenzEventArgs.Data[2].ToString("F2"),
                tenzEventArgs.Data[3].ToString("F2")
            });
        }

        public double[] CurrWeightKoefs { get; private set; }
    }

    class DataRxWinPresenter : Presenter
    {
        public DataRxWinPresenter(IStabilityModel model, IView view) : base(model, view)
        {
           view.ViewUpdated += ViewOnViewUpdated;
           
            ((DataRxWindow) _view).Closing +=
                (sender, args) =>
                {
                    _model.SetNewConfig(MainConfig.PortConfig, MainConfig.ExchangeConfig);
                    _model.UpdateWeightKoef -= ModelOnUpdateWeightKoef;
                };

            _model.UpdateWeightKoef += ModelOnUpdateWeightKoef;
        }

        private void ModelOnUpdateWeightKoef(object sender, TenzEventArgs tenzEventArgs)
        {
            ((Window)_view).Dispatcher.BeginInvoke(new Action(() =>
                MessageBox.Show((DataRxWindow) _view,
                    "Калибровка нуля проведена успешно. Новые параметры сохранены.", "Готово",MessageBoxButton.OK,MessageBoxImage.Information)));
        }

        private void ViewOnViewUpdated(object sender, EventArgs eventArgs)
        {
            CPortConfig portConf;
            StabilityExchangeConfig exchConf;
          ((DataRxWindow)_view).GetWinState(out portConf, out exchConf);
            _model.SetNewConfig(portConf,exchConf);
        }


    }
}
