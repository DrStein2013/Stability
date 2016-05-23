using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Stability.Enums;
using Stability.Model.Port;
using Stability.View;

namespace Stability.Model.Device
{
    [Serializable]
    public class DeviceDataEntry
    {
        public DeviceDataEntry(List<double[]> List)
        {
            AdcList = new List<double[]>();
            AdcList.AddRange(List);
        }

        public static DeviceDataEntry Deserialize(byte [] arrBytes)
        {
            var bin_format = new BinaryFormatter();
            var mem_stream = new MemoryStream();
            mem_stream.Write(arrBytes,0,arrBytes.Count());
            mem_stream.Position = 0;
            return (DeviceDataEntry)bin_format.Deserialize(mem_stream);
        }

        public static byte[] Serialize(DeviceDataEntry t)
        {
            var bin_format = new BinaryFormatter();
            var mem_stream = new MemoryStream();
            bin_format.Serialize(mem_stream, t);
            return mem_stream.ToArray();
        }

        public   ObservableCollection<cDeviceItem> GetCollection()
        {
            var res = new ObservableCollection<cDeviceItem>();
            for (int index = 0; index < AdcList.Count; index++)
            {
                var a = AdcList[index];
                res.Add(new cDeviceItem(a) { Num = index+1 });
            }
            return res;
        }

        public List<double[]> AdcList { get; private set; }
    }
    
    public class cDeviceItem
    {
       public cDeviceItem() { }
       public cDeviceItem(double[] mas)
       {
           CH_1 = mas[0];
           CH_2 = mas[1];
           CH_3 = mas[2];
           CH_4 = mas[3];
       }

        public int Num { get; set; }
        public double CH_1 { get; set; }
        public double CH_2 { get; set; }
        public double CH_3 { get; set; }
        public double CH_4 { get; set; }
    }

    public class WeightEventArgs : EventArgs
    {
        public double Weight { get; set; }
    }

    public class ProgressEventArgs : EventArgs
    {
        public int EntryCount { get; set; }    
        public double TimerCount { get; set; }
        public double[] Vals { get; set; }
    }

    public class StabilityExchangeConfig
    {
        public int Period { get; set; }
        public bool SavePureADCs { get; set; }
        public bool CorrectRxMistakes { get; set; }
        public InputFilterType FilterType { get; set; }
        public double[] AlphaBetaKoefs { get; set; }
        public StabilityExchangeConfig() {AlphaBetaKoefs = new double[4];}
    }

    class StabilityDevice : CDevice
    {
        public double[] CurrAdcVals { get;private set; }
        public volatile double[] WeightDoubles;// { get; private set; }
        private  StabilityParseMode _mode;

        public StabilityExchangeConfig ExchangeConfig { get; set; }
        
        private event EventHandler ParseDone;
        public event EventHandler  CalibrationDone;
        public event EventHandler<WeightEventArgs> WeightMeasured;
        public event EventHandler<DeviceEntryResponseArgs> MeasurementsDone;
        public event EventHandler<ProgressEventArgs> ProgressResp;

        private readonly List<double[]> _adcList = new List<double[]>();
        private readonly List<double[]> _weList = new List<double[]>();

        private int _zeroCalibrationCount = 100;

        private CalibrationParams _calibrationParams;
        public double[] WeightKoefs { get; set; }
        private double[] zeroAdcVals;
        private double weight;
        private byte w_count = 0;
        private double[] vl_prev = new double[4];
      
        private bool _isStarted;
        private bool _isRecording;
        private int _periodBuf;
        private bool _zeroCorrection = true;
        private bool _startupCalib;

        private long MeasurementCount;

        public StabilityDevice()
        {
            Port.RxEvent+=PortOnRxEvent;
            _mode = StabilityParseMode.ParseData;
            CurrAdcVals = new double[4];
            WeightKoefs = MainConfig.WeightKoefs;
            zeroAdcVals = MainConfig.ZeroAdcVals;
            ExchangeConfig = MainConfig.ExchangeConfig;

            WeightDoubles = new double[4];
        }

        private void OnParseDone(object sender, EventArgs e)
        {
            Thread.Sleep(ExchangeConfig.Period);
            SendCmd(new byte[] { 0x31 });
        }

        private void WeightCalc(object sender, EventArgs eventArgs)
        {
            if (_weList.Count < 20)
            {
                _weList.Add((double[]) WeightDoubles.Clone());
            }
            else
            {
                w_count++;
                if (w_count == 3)
                    ParseDone -= WeightCalc;
                
                double av1 = 0, av2 = 0, av3 = 0, av4 = 0;
                av1 = _weList.Average(doubles =>  Math.Round(doubles[0],1));
                av2 = _weList.Average(doubles =>  Math.Round(doubles[1],1));
                av3 = _weList.Average(doubles =>  Math.Round(doubles[2],1));
                av4 = _weList.Average(doubles =>  Math.Round(doubles[3],1));

                var w4 = (av1 + av2 + av3 + av4) / 4;
                weight += w4;
                _weList.Clear();
                WeightMeasured(this, new WeightEventArgs(){Weight = weight/w_count});
            }
        }

        public DeviceDataEntry GetDataEntry()
        {
            return new DeviceDataEntry(_adcList);
        }
     /*   private void RxThread()
        {
            while (true)
            {
                if (Port.GetRxBuf().Count > 0)
                {
                    var p = Port.GetRxBuf().Dequeue();
                    Parse(p);
                }
                Thread.Sleep(50);
            }
        }*/

        private void PortOnRxEvent(object sender, EventArgs eventArgs)
        {
            var p = Port.GetRxBuf().Dequeue();
            if(p.Data.Count > 0)
                Parse(p);
        }

        protected override void Parse(Pack pack)
        {
            switch (_mode)
            {
             case StabilityParseMode.ParseData:
                ParseData(pack);
             break;
             case StabilityParseMode.ParseCmd:
                ParseCmd(pack);
              break;
            }
            if(ParseDone != null)
                ParseDone.Invoke(this,null);//BeginInvoke(this, null, null, null);
        }
        
        private void ParseData(Pack pack)
        {
            var arr = new int[4];
          
            var barr = pack.Data.ToArray();
          //  arr = pack.Data.ToArray();
            for (int i = 0, j = 0; i < 4; i++, j += 2)
            {
                arr[i] = BitConverter.ToInt16(barr, j);
                arr[i] >>= 1;

            /*  if (ExchangeConfig.CorrectRxMistakes)
               {
                   if (arr[i] < 0x0F)
                       arr[i] = arr_prev[i];
                   else 
                       arr_prev[i] = arr[i];
                  
               }*/

              var vl = (arr[i] * 5.09 / 1024);
               
              if(ExchangeConfig.FilterType==InputFilterType.AlphaBeta)
                 vl = ExchangeConfig.AlphaBetaKoefs[i] * vl + (1 - ExchangeConfig.AlphaBetaKoefs[i]) * vl_prev[i];

              
                vl_prev[i] = vl;

                if (_zeroCorrection)
                {
                    if (Math.Abs(vl - zeroAdcVals[i]) < 0.06)
                        vl = 0.0;
                    else if (vl > zeroAdcVals[i])
                        vl -= zeroAdcVals[i];
                }

                if(ExchangeConfig.CorrectRxMistakes)
                    vl = Math.Round(vl, 2);

                CurrAdcVals[i] = vl;
      
                WeightDoubles[i] = CurrAdcVals[i]*WeightKoefs[i]; 

            }
         }

        public override void Calibrate(CalibrationParams calibParams, params object[] pObjects)
        {
            StopMeasurement();
            if (calibParams == null)
            {
                _zeroCorrection = false;
                _startupCalib = (bool)pObjects[0];
                _zeroCalibrationCount = (int) pObjects[1];
                ParseDone += ZeroCalibrationHandler;
            }
            else
            {
                if (!(bool) pObjects[0])
                {
                    ParseDone += WeightCalibrationHandler;
                    _calibrationParams = calibParams;
                    _periodBuf = ExchangeConfig.Period;
                    ExchangeConfig.Period = _calibrationParams.Period;
                }
                else
                {
                    ParseDone += FastWeightCalibrationHandler;
                    _calibrationParams = calibParams;
                }
            }
            StartMeasurement();
        }


        private void ParseCmd(Pack pack)
        {
            
        }

        public void GetWeight()
        {
            StopMeasurement();
            _weList.Clear();
            weight = 0;
            w_count = 0;
            ParseDone += WeightCalc;
            StartMeasurement();
        }

        public void StartRecording(int RecTime)
        {
            if (!_isRecording)
            {
                _isRecording = true;
                ParseDone += OnParseRecord;
                MeasurementCount = RecTime/MainConfig.ExchangeConfig.Period;
            }

        }

        public void StopRecording()
        {
            ParseDone -= OnParseRecord;
            _isRecording = false;
            MeasurementsDone(this, new DeviceEntryResponseArgs() { Data = new DeviceDataEntry(_adcList) });
        }

        private void OnParseRecord(object sender, EventArgs eventArgs)
        {
            var cnt = _adcList.Count;
            
            if (cnt < MeasurementCount)
                _adcList.Add((double[]) CurrAdcVals.Clone());
            else
                StopRecording();


            var Time = (cnt*MainConfig.ExchangeConfig.Period)/1000.0;
            ProgressResp(this, new ProgressEventArgs() { EntryCount = cnt, TimerCount = Time, Vals = (double[])CurrAdcVals.Clone() });
        }

        public void StartMeasurement()
        {
           if (!_isStarted)
            {
                ParseDone += OnParseDone;
                _adcList.Clear();
                Thread.Sleep(250);
                SendCmd(new byte[] {0x31});
                _isStarted = true;
            }
        }

        public void StopMeasurement()
        {
            if (_isStarted)
            {
                ParseDone -= OnParseDone;
                _isStarted = false;
            }
        }

        private void ZeroCalibrationHandler(object sender, EventArgs eventArgs)
        {
            if (_adcList.Count < _zeroCalibrationCount)
            {
                _adcList.Add((double[]) CurrAdcVals.Clone());
            }
            else
            {
                ParseDone -= ZeroCalibrationHandler;
                var zeroVals = new double[4];
            
                for (int i = 0; i < zeroVals.Count(); i++)
                {
                    zeroVals[i] = _adcList.Average(doubles => doubles[i]);
                    if (_startupCalib)
                    {
                        var w = 0.1;
                        var koef_new = w/zeroVals[i];
                        var koef_old = w/MainConfig.ZeroAdcVals[i];
                        var offset_percent = (100*koef_new)/koef_old;
                       
                        offset_percent = (100 - offset_percent)*10;
                        

                        WeightKoefs[i] += (WeightKoefs[i] * offset_percent) / 100;
                    }
                }

                if (!_startupCalib)
                    MainConfig.Update(null, zeroVals);
                zeroAdcVals = zeroVals;

                if (CalibrationDone != null)
                    CalibrationDone.Invoke(null, null);
                _zeroCorrection = true;
                _startupCalib = false;
                _adcList.Clear();
            }
            //------------
        }

        private void WeightCalibrationHandler(object o, EventArgs eventArgs)
        {
            if (_adcList.Count < _calibrationParams.EntryCount)
            {
                _adcList.Add((double[]) CurrAdcVals.Clone());
            }
            else
            {
                ParseDone -= WeightCalibrationHandler;
                var aver=_adcList.Average(val => val[_calibrationParams.TenzNumber]);
                double koef = _calibrationParams.Weight/aver;
                WeightKoefs[_calibrationParams.TenzNumber] = koef;
                if (CalibrationDone != null) CalibrationDone.Invoke(null, null);
                _adcList.Clear();
                _calibrationParams = null;
                ExchangeConfig.Period = _periodBuf;
            }
        }

        private void FastWeightCalibrationHandler(object o, EventArgs eventArgs)
        {
            if (_adcList.Count < _calibrationParams.EntryCount)
            {
                _adcList.Add((double[])CurrAdcVals.Clone());
            }
            else
            {
                ParseDone -= FastWeightCalibrationHandler;
                for (int i = 0; i < 4; i++)
                {
                    var aver = _adcList.Average(val => val[i]);
                    double koef = _calibrationParams.Weight / aver;
                    WeightKoefs[i] = koef;   
                }

                if (CalibrationDone != null) CalibrationDone.Invoke(null, null);
                _adcList.Clear();
                _calibrationParams = null;
            }  
        }

    }
}