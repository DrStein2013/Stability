using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Stability.Model.Port;
using Stability.View;

namespace Stability.Model.Device
{
    enum StabilityParseMode
    {
        ParseData=0, ParseCmd
    }

    class StabilityDevice : CDevice
    {
        public double[] CurrAdcVals { get;private set; }
        
        private StabilityParseMode _mode;

      //  private event EventHandler _parseDone;
        public event EventHandler calibrationDone; 

        private List<double[]> _adcList;

        private int ZeroCalibrationCount = 100;

        public double[] _weighKoefs { get; private set; }
        public StabilityDevice()
        {
            Port.RxEvent+=PortOnRxEvent;
            _mode = StabilityParseMode.ParseData;
            CurrAdcVals = new double[4];
            _weighKoefs = MainConfig.WeightKoefs;
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
           /* if(_parseDone != null)
                _parseDone.Invoke(this,null);//BeginInvoke(this, null, null, null);*/
        }

        private void ParseData(Pack pack)
        {
            var zeroAdcVals = MainConfig.ZeroAdcVals;
            var arr = new int[4];
            var barr = pack.Data.ToArray();
            for (int i = 0, j = 0; i < 4; i++, j += 2)
            {
                arr[i] = BitConverter.ToInt16(barr, j);
                CurrAdcVals[i] = (arr[i]*5.0/1024)-zeroAdcVals[i];
            }
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

            while(list.Count < ZeroCalibrationCount)
            {
                SendCmd(new byte[] {0x31});
                Thread.Sleep(100);
                list.Add(CurrAdcVals);
            }

            foreach (var val in list)
            {
                zeroAdcVals[0] += val[0];
                zeroAdcVals[1] += val[1];
                zeroAdcVals[2] += val[2];
                zeroAdcVals[3] += val[3];
            }
            for (int i = 0; i < zeroAdcVals.Count(); i++)
                zeroAdcVals[i] /= ZeroCalibrationCount;

            //------------
        }

        private void WeightCalibrationHandler(object o)
        {
            var param = (CalibrationParams) o;
            double koef,aver=0;

            var list = new List<double[]>();
            while (list.Count < param.EntryCount)
            {
                SendCmd(new byte[] { 0x31 });
                Thread.Sleep(param.Period);
                list.Add(CurrAdcVals);
            }
            aver = list.Average(val => val[param.TenzNumber]);
            koef = param.Weight/aver;
            _weighKoefs[param.TenzNumber] = koef;
            if (calibrationDone != null) calibrationDone.Invoke(null, null);
        }
    }
}