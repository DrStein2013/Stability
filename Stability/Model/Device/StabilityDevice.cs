using System;
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

        public StabilityDevice()
        {
            Port.RxEvent+=PortOnRxEvent;
            _mode = StabilityParseMode.ParseData;
            CurrAdcVals = new double[4];
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

        }

        private void ParseData(Pack pack)
        {
            var arr = new int[4];
            var barr = pack.Data.ToArray();
            for (int i = 0, j = 0; i < 4; i++, j += 2)
            {
                arr[i] = BitConverter.ToInt16(barr, j);
                CurrAdcVals[i] = arr[i]*5.0/1024;
            }
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

    }
}