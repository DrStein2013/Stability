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

    public abstract class DateBaseResponseArg : EventArgs
    {
        public BaseAction Action { get; set; }
        public bool Error { get; set; }
        public string Response { get; set; }
    }

    public class PatientModelResponseArg : DateBaseResponseArg
    {
        public long ID { get; set; }
        public cPatient Patient { get; set; }
        public PatientBaseDataSet.Pat_TabDataTable PatientTable { get; set; }
    }

    public class AnamnesisModelResponseArg : DateBaseResponseArg
    {
       public PatientBaseDataSet.AnamnesisDataTable Table { get; set; }
       public BaseEntryState EntryState { get; set; }
       public object[] EntryObjects;    //0 - string, 1 - weight;
    }

   public class DeviceEntryResponseArgs : EventArgs
    {
        public DeviceDataEntry Data { get; set; }
    }

    public class GraphEntryResponseArgs : EventArgs
    {
        public double Time { get; set; }
        public double[] Points { get; set; }
    }

  
   public interface IStabilityModel
    {
        event EventHandler<TenzEventArgs> UpdateDataView;
        event EventHandler<TenzEventArgs> UpdateWeightKoef;
        event EventHandler<WeightEventArgs> UpdateWeight;
        event EventHandler<PatientModelResponseArg> UpdatePatient;
        event EventHandler<AnamnesisModelResponseArg> UpdateAnamnesis;
        event EventHandler<DeviceEntryResponseArgs> UpdateDataEntry;
        event EventHandler<ProgressEventArgs> UpdateProgress;
       // event EventHandler<GraphEntryResponseArgs> GraphUpdate;

        void DeviceCmdFromView(DeviceCmdArgEvent c);
        void PatientEventFromView(PatientModelResponseArg p);
        void AnamnesisEventFromView(AnamnesisModelResponseArg p);
       
        void SetNewConfig(CPortConfig c, StabilityExchangeConfig stabilityExchangeConfig);
    }

    public class StabilityModel : IStabilityModel
    {
        private readonly StabilityDevice _device;
        //private StabilityAnalyzer _analyzer;
        private readonly cDataBase _base;
        private cPatient _currentPatient;
        private long _currentPatientId;
        private PatientBaseDataSet.AnamnesisDataTable _currentPatAnamnesis;

        private BaseEntryState _baseEntryState;

        public event EventHandler<TenzEventArgs> UpdateDataView;
        public event EventHandler<TenzEventArgs> UpdateWeightKoef;
        public event EventHandler<WeightEventArgs> UpdateWeight;
        public event EventHandler<PatientModelResponseArg> UpdatePatient;
        public event EventHandler<AnamnesisModelResponseArg> UpdateAnamnesis;
        public event EventHandler<DeviceEntryResponseArgs> UpdateDataEntry;
        public event EventHandler<ProgressEventArgs> UpdateProgress;
        //public event EventHandler<GraphEntryResponseArgs> GraphUpdate;

        public bool ShowAdcs { get; set; }
        private readonly Timer _viewUpdaterTimer;

        public StabilityModel()
        {
            var conf = MainConfig.PortConfig;
            IoC.GetKernel().Bind<IPort>().To<CComPort>().InSingletonScope().WithConstructorArgument("config", conf);
            _device = new StabilityDevice();
            _base = new cDataBase();
            _baseEntryState = BaseEntryState.Empty;
           
            _device.CalibrationDone +=
                (sender, args) =>
                {
                    if (UpdateWeightKoef != null)
                        UpdateWeightKoef(this, new TenzEventArgs() {Data = _device.WeightKoefs});
                };

            _device.WeightMeasured+=DeviceOnWeightMeasured;

            //На ивент ниже необходимо подписать функцию, которая перегонит данные из АЦП в 
            //класс анализа, откуда уже можно брать данные для графиков и т.д.
            _device.MeasurementsDone += (sender, args) => UpdateDataEntry.BeginInvoke(sender, args, null, null);
            _device.MeasurementsDone += (sender, args) => _baseEntryState = BaseEntryState.New;
            _device.ProgressResp += (sender, args) => UpdateProgress.BeginInvoke(sender, args, null, null);
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
                    case DeviceCmd.START_RECORDING:
                        _device.StartRecording(deviceCmdArgEvent.MeasureTime);
                    break;
                    case DeviceCmd.STOP_RECORDING:
                        _device.StopRecording();
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

        public void PatientEventFromView(PatientModelResponseArg p)
        {
            switch (p.Action)
            {
               case BaseAction.AddPatient:
                    long id=0;
                    if (_base.AddPatient(p.Patient, ref id))
                    {
                        p.Response = "Новый пациент успешно добавлен";
                        _currentPatient = p.Patient;
                    }
                    else
                    {
                        p.Response = "Такой пациент уже существует в базе";
                        p.Error = true;
                    }
                    p.ID = id;
                    
                    if(UpdatePatient!=null)
                        UpdatePatient.Invoke(this,p);
                break;
               case BaseAction.Find:
                    var tab = new PatientBaseDataSet.Pat_TabDataTable();
                    var pat = _base.FindPatientBy(p.ID,ref tab);
                    if (pat == null)
                    {
                        p.Error = true;
                        p.Response = "Пациент с таким ID отсутствует в базе";
                    }
                    p.Patient = pat;
                    p.PatientTable = tab;
                    _currentPatientId = p.ID;
                    if(UpdatePatient!=null)
                        UpdatePatient.Invoke(this, p);
                break;
            }
        }

        public void AnamnesisEventFromView(AnamnesisModelResponseArg p)
        {
            switch (p.Action)
            {
                    case BaseAction.Find:
                       var tab =  _base.GetAnamesisRangeBy(_currentPatientId);
                       if (tab.Count == 0)
                       {
                           p.Error = true;
                           p.Response = "Замеры для текущего пациента отсутствуют";
                       }
                        p.Table = tab;
                        _currentPatAnamnesis = tab;
                        if(UpdateAnamnesis!= null)
                            UpdateAnamnesis.Invoke(this,p);
                    break;
                    case BaseAction.AddNewEntry:
                        if (_baseEntryState == BaseEntryState.New)
                        {
                            var inf = (string) p.EntryObjects[0];
                            var weight = (double) p.EntryObjects[1];
                            var entry = new cAnamnesisEntry()
                            {
                                Entry = _device.GetDataEntry(),
                                Info = inf,
                                MeasureDate = DateTime.Now,
                                Weight = weight
                            };
                            _base.AddAnamnesis(entry, _currentPatientId);
                            p.Action = BaseAction.Find;
                            AnamnesisEventFromView(p);
                            _baseEntryState = BaseEntryState.Loaded;
                        }
                    break;
                    case BaseAction.ClearEntry:
                        if (_baseEntryState == BaseEntryState.New)
                        {
                            if (p.EntryState == BaseEntryState.New)
                            {
                                p.EntryState = _baseEntryState;
                                p.Response = "Желаете сохранить текущие данные?";
                                p.Error = false;
                                if (UpdateAnamnesis != null)
                                    UpdateAnamnesis.Invoke(this, p);
                            }
                            else if(p.EntryState == BaseEntryState.Empty)
                                _baseEntryState = BaseEntryState.Empty;
                         }
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
