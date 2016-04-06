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
        
        private event EventHandler ParseDone;
        public event EventHandler  CalibrationDone;
        public event EventHandler<WeightEventArgs> WeightMeasured;

        private readonly List<double[]> _adcList = new List<double[]>();
        private readonly List<double[]> _weList = new List<double[]>();

        private int ZeroCalibrationCount = 100;

        private CalibrationParams _calibrationParams;
        public double[] WeightKoefs { get; set; }
        private double weight;
        private byte w_count = 0;
        private double[] vl_prev = new double[4];
        private double[] w_prev = new double[4];
        private byte[] arr_prev = new byte[4] {35,35,35,35};

        private bool _isStarted;
        private int _periodBuf;
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
            if (_weList.Count < 20)
            {
                _weList.Add((double[]) WeightDoubles.Clone());
               // OnParseDone(sender,eventArgs);
                //      Thread.Sleep(ExchangeConfig.Period);
                //      SendCmd(new byte[] { 0x31 });
                //      return;
            }
            else
            {
                w_count++;
                if (w_count == 3)
                    ParseDone -= WeightCalc;
                
                double av1 = 0, av2 = 0, av3 = 0, av4 = 0;
                av1 = _weList.Average(doubles => doubles[0]);
                av2 = _weList.Average(doubles => doubles[1]);
                av3 = _weList.Average(doubles => doubles[2]);
                av4 = _weList.Average(doubles => doubles[3]);

                var w2 = (av1 + av4)/2;
                
                var w3 = (av1 + av4 + av3)/3;
                var w4 = (av1 + av2 + av3 + av4) / 4;
                weight += w4;
                _weList.Clear();
                WeightMeasured(this, new WeightEventArgs(){Weight = weight/w_count});
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
            var zeroAdcVals = MainConfig.ZeroAdcVals;
            var arr = new byte[4];
          
            //var barr = pack.Data.ToArray();
            arr = pack.Data.ToArray();
            for (int i = 0, j = 0; i < 4; i++, j += 2)
            {
               // arr[i] = BitConverter.ToInt16(barr, j);
               // arr[i] >>= 2;

              if (ExchangeConfig.CorrectRxMistakes)
               {
                   if (arr[i] < 0x0F)
                       arr[i] = arr_prev[i];
                   else 
                       arr_prev[i] = arr[i];
                   /*if ((vl < 0.1)&&(i==1))
                       vl = vl_prev[i];*/
               }

              var vl = (arr[i] * 5.09 / 256);
               
              if(ExchangeConfig.FilterType==InputFilterType.AlphaBeta)
              vl = ExchangeConfig.AlphaBetaKoefs[i] * vl + (1 - ExchangeConfig.AlphaBetaKoefs[i]) * vl_prev[i];

                vl_prev[i] = vl;

                if (Math.Abs(vl - zeroAdcVals[i]) < 0.1)
                    vl = 0.0;
                else if (vl > zeroAdcVals[i])
                    vl -= zeroAdcVals[i];
                
                CurrAdcVals[i] = vl;
             //   var a = 0.2;
                WeightDoubles[i] = CurrAdcVals[i]*MainConfig.WeightKoefs[i];//*a+w_prev[i]*(1-a);
                //  w_prev[i] = WeightDoubles[i];
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
                _periodBuf = ExchangeConfig.Period;
                ExchangeConfig.Period = _calibrationParams.Period;
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
               // SendCmd(new byte[] {0x30});
                _isStarted = false;
            }
        }

        private void ZeroCalibrationHandler(object sender, EventArgs eventArgs)
        {
          
            if (_adcList.Count < ZeroCalibrationCount)
            {
                _adcList.Add((double[]) CurrAdcVals.Clone());
                //   Thread.Sleep(ExchangeConfig.Period);
                //   SendCmd(new byte[] { 0x31 });
            }
            else
            {
                ParseDone -= ZeroCalibrationHandler;
                var zeroAdcVals = new double[4];
                for (int i = 0; i < zeroAdcVals.Count(); i++)
                    zeroAdcVals[i] = _adcList.Average(doubles => doubles[i]);
                
                /*foreach (var val in _adcList)
                {
                 //   zeroAdcVals[0] += val[0];
                    zeroAdcVals[1] += val[1];
                    zeroAdcVals[2] += val[2];
                    zeroAdcVals[3] += val[3];
                }
                for (int i = 1; i < zeroAdcVals.Count(); i++)
                    zeroAdcVals[i] /= ZeroCalibrationCount;*/

                MainConfig.Update(null, zeroAdcVals);
                if (CalibrationDone != null)
                    CalibrationDone.Invoke(null, null);
                _adcList.Clear();
            }
            //------------
        }

        private void WeightCalibrationHandler(object o, EventArgs eventArgs)
        {
          //  var param = (CalibrationParams) o;

            //  var list = new List<double[]>();
            if (_adcList.Count < _calibrationParams.EntryCount)
            {
                _adcList.Add((double[]) CurrAdcVals.Clone());
                //  Thread.Sleep(ExchangeConfig.Period);
                //  SendCmd(new byte[] { 0x31 });
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
                ExchangeConfig.Period = _periodBuf; // MainConfig.ExchangeConfig.Period;
            }
            // StartMeasurement();
        }
    }
}