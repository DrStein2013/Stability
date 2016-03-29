using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Stability.Enums;
using Stability.Model.Port;
using Stability.View;

namespace Stability.Model.Device
{
    public class WeightEventArgs : EventArgs
    {
        public double Weight { get; set; }
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
        
        private event EventHandler<EventArgs> ParseDone;
        public event EventHandler  CalibrationDone;
        public event EventHandler<WeightEventArgs> WeightMeasured;

        private readonly List<double[]> _adcList = new List<double[]>();
        private readonly List<double[]> _weList = new List<double[]>();

        private int ZeroCalibrationCount = 100;

        private CalibrationParams _calibrationParams;
        public double[] WeightKoefs { get; set; }
        private double weight;
        private double[] vl_prev = new double[4];
        private double[] arr_prev = new double[4] {0.4, 0.4, 0.4, 0.4};
        
        public StabilityDevice()
        {
            Port.RxEvent+=PortOnRxEvent;
            _mode = StabilityParseMode.ParseData;
            CurrAdcVals = new double[4];
            WeightKoefs = MainConfig.WeightKoefs;
            ExchangeConfig = MainConfig.ExchangeConfig;
            
            WeightDoubles = new double[4];
           
       //     var t = new Thread(OnParseDone) {Priority = ThreadPriority.Highest, IsBackground = true};
       //     t.Start();

   //         var t2 = new Thread(RxThread) {Priority = ThreadPriority.AboveNormal, IsBackground = true};
   //         t2.Start();
        }

        private void OnParseDone(object sender, EventArgs e)
        {
            Thread.Sleep(ExchangeConfig.Period);
            SendCmd(new byte[] { 0x31 });
        }

        private void WeightCalc(object sender, EventArgs eventArgs)
        {
            if (_weList.Count < 50)
            {
                _weList.Add((double[]) WeightDoubles.Clone());
                //      Thread.Sleep(ExchangeConfig.Period);
                //      SendCmd(new byte[] { 0x31 });
                //      return;
            }
            else
            {
                ParseDone -= WeightCalc;
                double av1 = 0, av2 = 0, av3 = 0, av4 = 0;
                av1 = _weList.Average(doubles => doubles[0]);
                av2 = _weList.Average(doubles => doubles[1]);
                av3 = _weList.Average(doubles => doubles[2]);
                av4 = _weList.Average(doubles => doubles[3]);

                var w2 = (av1 + av4)/2;
                w2 += 1;
                var w3 = (av1 + av4 + av3)/3;
                w3 += 1;
                weight = (av1 + av2 + av3 + av4)/4;
                _weList.Clear();
                WeightMeasured(this, new WeightEventArgs(){Weight = weight});
            }
        }


        private void RxThread()
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
        }

        private void PortOnRxEvent(object sender, EventArgs eventArgs)
        {
            var p = Port.GetRxBuf().Dequeue();
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
            var zeroAdcVals = MainConfig.ZeroAdcVals;
            var arr = new int[4];
          
            var barr = pack.Data.ToArray();
            for (int i = 0, j = 0; i < 4; i++, j += 2)
            {
                arr[i] = BitConverter.ToInt16(barr, j);

                var vl = (arr[i] * 5.09 / 1024);

                if (ExchangeConfig.CorrectRxMistakes)
                {
                    if (vl < 0.4)
                        vl = arr_prev[i];
                    else if (Math.Abs(vl - arr_prev[i]) < 0.5)
                        arr_prev[i] = vl;
                }


            if (vl < zeroAdcVals[i])
              vl = 0.0;
            else
              vl -= zeroAdcVals[i];
                
            if(ExchangeConfig.FilterType==InputFilterType.AlphaBeta)
              vl = ExchangeConfig.AlphaBetaKoefs[i] * vl + (1 - ExchangeConfig.AlphaBetaKoefs[i]) * vl_prev[i];

                vl_prev[i] = vl;

                CurrAdcVals[i] = vl;
                WeightDoubles[i] = CurrAdcVals[i]*MainConfig.WeightKoefs[i];
            }
         }

        public override void Calibrate(CalibrationParams calibParams)
        {
            StopMeasurement();
            if (calibParams == null)
            {
                ParseDone += ZeroCalibrationHandler;
            }
            else
            {
                ParseDone += WeightCalibrationHandler;
                _calibrationParams = calibParams;
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
            ParseDone += WeightCalc;
            StartMeasurement();
        }

        public void StartMeasurement()
        {
            ParseDone += OnParseDone;
            _adcList.Clear();
            SendCmd(new byte[] { 0x31 });
        }

        public void StopMeasurement()
        {
            ParseDone -= OnParseDone;
            SendCmd(new byte[] { 0x30 });
        }

        private void ZeroCalibrationHandler(object sender, EventArgs eventArgs)
        {
           //_adcList.Add(CurrAdcVals);
            var zeroAdcVals = new double[4];
            var list = new List<double[]>();

            while(_adcList.Count < ZeroCalibrationCount)
            {
                _adcList.Add((double[])CurrAdcVals.Clone());
                return;
             //   Thread.Sleep(ExchangeConfig.Period);
             //   SendCmd(new byte[] { 0x31 });
            }

            ParseDone -= ZeroCalibrationHandler;
            foreach (var val in _adcList)
            {
                zeroAdcVals[0] += val[0];
                zeroAdcVals[1] += val[1];
                zeroAdcVals[2] += val[2];
                zeroAdcVals[3] += val[3];
            }
            for (int i = 0; i < zeroAdcVals.Count(); i++)
                zeroAdcVals[i] /= ZeroCalibrationCount;
            MainConfig.Update(null,zeroAdcVals);
            if(CalibrationDone != null)
                CalibrationDone.Invoke(null,null);
            _adcList.Clear();
            //------------
        }

        private void WeightCalibrationHandler(object o, EventArgs eventArgs)
        {
          //  var param = (CalibrationParams) o;
            double aver=0;

          //  var list = new List<double[]>();
            while (_adcList.Count < _calibrationParams.EntryCount)
            {
                _adcList.Add((double[])CurrAdcVals.Clone());
                return;
              //  Thread.Sleep(ExchangeConfig.Period);
              //  SendCmd(new byte[] { 0x31 });
            }
            ParseDone -= WeightCalibrationHandler;
            aver = _adcList.Average(val => val[_calibrationParams.TenzNumber]);
            double koef = _calibrationParams.Weight / aver;
            WeightKoefs[_calibrationParams.TenzNumber] = koef;
            if (CalibrationDone != null) CalibrationDone.Invoke(null, null);
            _adcList.Clear();
            _calibrationParams = null;
            // StartMeasurement();
        }
    }
}