using System.Collections.Generic;
using Stability.Enums;
using Stability.Model.Device;

namespace Stability.Model.Analyzer
{
     class Analyzer 
    {

        public virtual List<double[]> Filter(FilterType type, params int[]fltParams)
        {
           throw new System.NotImplementedException();
        }

        protected virtual void FFT()
        {
            throw new System.NotImplementedException();
        }

    }

    class StabilityAnalyzer : Analyzer
    {
        public List<double[]> PureTenzoList
        {  set { _pureTenzoList = value;
              _tenzoList = value;
            //  value.CopyTo(_tenzoList.ToArray());
            } 
        }

        private List<double[]> _pureTenzoList;
        private List<double[]> _tenzoList;
        private List<double[]> _stabilogramsList = new List<double[]>();
        private List<double[]> _fftList = new List<double[]>();

        public override List<double[]> Filter(FilterType type, params int[] fltParams)
        {
            //fltParams[0] - target list to filter
            //fltParams[1] - window for SMA
            var flt = new Filter();
            var targetList = fltParams[0];
            switch (targetList)
            {
                case 0:
                     _tenzoList = flt.CalcFilter(_tenzoList, type, fltParams[1]);
                    return _tenzoList;
                    break;
                case 1:
                    _stabilogramsList = flt.CalcFilter(_stabilogramsList, type, fltParams[1]);
                    return _stabilogramsList;
                    break;
                default:
                    break;
            }
            return null;
        }

        public DeviceDataEntry Calculate(GraphTypes type)
        {
            switch (type)
            {
              case GraphTypes.StabilogramVals:
                    return GetStabilograms();
              break;
            }
            return new DeviceDataEntry(null);
        }

        public DeviceDataEntry ResetLists()
        {
            _tenzoList.Clear();
            _tenzoList = _pureTenzoList;
           // _pureTenzoList.CopyTo(_tenzoList.ToArray()); 
            _stabilogramsList.Clear();
            _fftList.Clear();
            return new DeviceDataEntry(_pureTenzoList);
        }

        private DeviceDataEntry GetStabilograms()
        {
            return new DeviceDataEntry(null);
        }
    }
    
}