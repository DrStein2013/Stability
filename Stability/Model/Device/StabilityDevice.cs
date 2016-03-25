using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Stability.Enums;
using Stability.Model.Port;
using Stability.View;

namespace Stability.Model.Device
{
  
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
        private StabilityParseMode _mode;

        public StabilityExchangeConfig ExchangeConfig { get; set; }
        
        private event EventHandler _parseDone;
        public event EventHandler calibrationDone; 

        private List<double[]> _adcList = new List<double[]>();
        private List<double[]> _weList = new List<double[]>();

        private int ZeroCalibrationCount = 100;

        public double[] _weighKoefs { get; set; }
        private double weight;
        private double[] vl_prev = new double[4];
        private int[] arr_prev = new int[4] {30,30,30,30};
        //private double[] wDoubles = new double[4]{1.0,0.5,0.5,1.0};

        public StabilityDevice()
        {
        //    Port.RxEvent+=PortOnRxEvent;
            _mode = StabilityParseMode.ParseData;
            CurrAdcVals = new double[4];
            _weighKoefs = MainConfig.WeightKoefs;
            ExchangeConfig = MainConfig.ExchangeConfig;

            WeightDoubles = new double[4];
          //  _parseDone+= OnParseDone;

            var t = new Thread(OnParseDone) {Priority = ThreadPriority.Highest, IsBackground = true};
            t.Start();

            var t2 = new Thread(RxThread) {Priority = ThreadPriority.AboveNormal, IsBackground = true};
            t2.Start();
        }

        private void OnParseDone(/*object sender, EventArgs eventArgs*/)
        {
            StopMeasurement();
            Thread.Sleep(1000);
            double aver_w = 0;
            for (int i = 0; i < 10; i++)
            {
                while (_weList.Count < 50)
                {
                    SendCmd(new byte[] {0x31});
                    Thread.Sleep(ExchangeConfig.Period);
                    // _adcList.Add(WeightDoubles);
                }

                //StopMeasurement();
                //if (_adcList.Count > 500)
                // {

                //   _parseDone -= OnParseDone;
                double av1 = 0, av2 = 0, av3 = 0, av4 = 0;
                foreach (var entry in _weList)
                {
                    av1 += entry[0];
                    av2 += entry[1];
                    av3 += entry[2];
                    av4 += entry[3];
                }
                av1 /= _weList.Count;
                av2 /= _weList.Count;
                av3 /= _weList.Count;
                av4 /= _weList.Count;
                var w2 = (av1 + av4);///2;
                w2 += 1;
                var w3 = (av1 + av4 + av3)/3;
                w3 += 1;
                weight = (av1 + av2 + av3 + av4)/4;
                aver_w += weight;
                _weList.Clear();
           }
            aver_w /= 10;
            aver_w += 0;
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
            if(_parseDone != null)
                _parseDone.Invoke(this,null);//BeginInvoke(this, null, null, null);
        }

        private void ParseData(Pack pack)
        {
            var zeroAdcVals = MainConfig.ZeroAdcVals;
            var arr = new int[4];
          
            var barr = pack.Data.ToArray();
            for (int i = 0, j = 0; i < 4; i++, j += 2)
            {
                arr[i] = BitConverter.ToInt16(barr, j);

                if (ExchangeConfig.CorrectRxMistakes)
                {
                    if (arr[i] < 30)   
                        arr[i] = arr_prev[i];
                    else
                        arr_prev[i] = arr[i]; 
                }

                var vl = (arr[i]*5.09/1024);

               /* if ((vl - zeroAdcVals[i]) > 0.01)
                    vl -= zeroAdcVals[i];*/
                if (vl < zeroAdcVals[i])
                    vl = 0.0;
                else// if ((vl - zeroAdcVals[i]) > 0.1)
                    vl -= zeroAdcVals[i];
                
            if(ExchangeConfig.FilterType==InputFilterType.AlphaBeta)
              vl = ExchangeConfig.AlphaBetaKoefs[i] * vl + (1 - ExchangeConfig.AlphaBetaKoefs[i]) * vl_prev[i];

              /*  if(ExchangeConfig.CorrectRxMistakes)
                {
                    if (Math.Abs(vl - (5.09/1024)) < 0.1)
                        vl = vl_prev[i];
                    else
                        vl_prev[i] = vl;
                }*/

                CurrAdcVals[i] = vl;//(arr[i]*5.0/1024)-zeroAdcVals[i];
                WeightDoubles[i] = CurrAdcVals[i]*MainConfig.WeightKoefs[i];
            }
            _weList.Add((double[]) WeightDoubles.Clone());
            _adcList.Add((double[])CurrAdcVals.Clone());
        }

        public override void Calibrate(CalibrationParams calibrationParams)
        {
            StopMeasurement();
            if (calibrationParams == null)
            {
                var thr = new Thread(ZeroCalibrationHandler)
                {
                    Priority = ThreadPriority.AboveNormal,
                    IsBackground = true
                };
                thr.Start();
            }
            else
            {
                var thr = new Thread(WeightCalibrationHandler)
                {
                    Priority = ThreadPriority.AboveNormal,
                    IsBackground = true
                };
                thr.Start(calibrationParams);
            }
            /*      _adcList = new List<double[]>();
            ZeroAdcVals = new double[4];
            _parseDone += ZeroCalibrationHandler;*/
        }


        private void ParseCmd(Pack pack)
        {
            
        }

        public void StartMeasurement()
        {
            SendCmd(new byte[] { 0x32 });
        }

        public void StopMeasurement()
        {
            SendCmd(new byte[] { 0x30 });
        }

        private void ZeroCalibrationHandler()
        {
           //_adcList.Add(CurrAdcVals);
            var zeroAdcVals = new double[4];
            var list = new List<double[]>();

            while(_adcList.Count < ZeroCalibrationCount)
            {
                SendCmd(new byte[] {0x31});
                Thread.Sleep(ExchangeConfig.Period);
              //  list.Add((double[])CurrAdcVals.Clone());
            }

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
            _adcList.Clear();
            //------------
        }

        private void WeightCalibrationHandler(object o)
        {
            var param = (CalibrationParams) o;
            double koef,aver=0;

            var list = new List<double[]>();
            while (_adcList.Count < param.EntryCount)
            {
                SendCmd(new byte[] { 0x31 });
                Thread.Sleep(param.Period);
                //list.Add((double[]) CurrAdcVals.Clone());
            }
            aver = _adcList.Average(val => val[param.TenzNumber]);
            koef = param.Weight/aver;
            _weighKoefs[param.TenzNumber] = koef;
            if (calibrationDone != null) calibrationDone.Invoke(null, null);
            _adcList.Clear();
           // StartMeasurement();
        }
    }
}