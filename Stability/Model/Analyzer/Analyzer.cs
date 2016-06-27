﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;
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
        public double[] W_k { get; set; }
        public double Weight { get; set; }
        public List<double[]> PureTenzoList
        {  set { _pureTenzoList = value;
              _tenzoList = value;
            //  value.CopyTo(_tenzoList.ToArray());
            } 
        }

        private readonly double g = 9.8;    //Ускорение свободного падения
        private readonly double R = 0.175;  //Растояние от цетра окружности, описаной вокруг платформы весов, до датчика, м

        private List<double[]> _pureTenzoList;
        private List<double[]> _tenzoList;
        private List<double[]> _stabilogramsList = new List<double[]>();
        private List<Complex[]> _fftList = new List<Complex[]>();

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
              case GraphTypes.TenzoVals:
                    return ResetLists();
              case GraphTypes.StabilogramVals:
                    return GetStabilograms();
              case GraphTypes.TenzoFFT:
                    return GetTenzoFFT();
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

        private DeviceDataEntry GetTenzoFFT()
        {
            var cnt = _tenzoList[0].Count();
            for (int j = 0; j < cnt; j++)
            {
                var complex = new List<Complex>(); //new MathNet.Numerics.Complex32[_tenzoList.Count];
                for (int i = 0; i < _tenzoList.Count; i++)
                {
                    complex.Add(new Complex(_tenzoList[i][0], 1.0));
                }
                MathNet.Numerics.IntegralTransforms.Fourier.Forward(complex.ToArray());

                _fftList.Add(complex.ToArray());
            }
            return null; //new DeviceDataEntry(_fftList);
        }
        private DeviceDataEntry GetStabilograms()
        {
            _stabilogramsList.Clear();
            var kx = W_k[0] - W_k[2];
            var ky = W_k[3] - W_k[1];

            var KX = R/kx*g;
            var KY = R/ky*g;

            foreach (var V in _tenzoList)
            {
                var Vxi = V[0] - V[2];
                var Vyi = V[3] - V[1];

                var xi = KY*Vyi/Weight;
                var yi = KX*Vxi/Weight;

                _stabilogramsList.Add(new double[] { xi, yi });
            }
         /*   var masx = new double[600];
            var masy = new double[600];
            for (int i = 0; i < 600; i++)
            {
                masx[i] = _stabilogramsList[i][0];
                masy[i] = _stabilogramsList[i][1];
            }*/
            return new DeviceDataEntry(_stabilogramsList);
        }
    }
    
}