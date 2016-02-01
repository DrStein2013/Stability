using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Stability.Model.Port;

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

        private double[] ZeroAdcVals;

        private List<double[]> _adcList;

        private int ZeroCalibrationCount = 100;

        public StabilityDevice()
        {
            Port.RxEvent+=PortOnRxEvent;
            _mode = StabilityParseMode.ParseData;
            CurrAdcVals = new double[4];
            ZeroAdcVals = new double[4];
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
            var arr = new int[4];
            var barr = pack.Data.ToArray();
            for (int i = 0, j = 0; i < 4; i++, j += 2)
            {
                arr[i] = BitConverter.ToInt16(barr, j);
                CurrAdcVals[i] = (arr[i]*5.0/1024)-ZeroAdcVals[i];
            }
        }

        public override void Calibrate()
        {
            StopMeasurement();
            ZeroAdcVals = new double[4];
            var thr = new Thread(ZeroCalibrationHandler) {Priority = ThreadPriority.AboveNormal, IsBackground = true};
            thr.Start();
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
            var list = new List<double[]>();

            while(list.Count < ZeroCalibrationCount)
            {
                SendCmd(new byte[] {0x31});
                Thread.Sleep(100);
                list.Add(CurrAdcVals);
            }

            foreach (var val in list)
            {
                ZeroAdcVals[0] += val[0];
                ZeroAdcVals[1] += val[1];
                ZeroAdcVals[2] += val[2];
                ZeroAdcVals[3] += val[3];
            }
            for (int i = 0; i < ZeroAdcVals.Count(); i++)
                ZeroAdcVals[i] /= ZeroCalibrationCount;
        }
    }
}