using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Stability.Enums;

namespace Stability.Model.Analyzer
{
    public class Filter
    {
        public List<double[]> CalcFilter(List<double[]> input, FilterType type, params int[] fltParams)
        {
            switch (type)
            {
               case FilterType.MovingAverage:
                    return MovingAverage(fltParams[0], input);
                    break;
            }
            return null;
        }


        private List<double[]> MovingAverage(int window,List<double[]> input)
        {
            var interval = new List<double[]>(window);
            List<double[]> MAs = new List<double[]>();
            
            for (int i = 0; i < input.Count; i++)
             {
                 if (i > input.Count - window)                
                     window--;
                
                 for (int j = 0; j < window; j++)
                 {
                     interval.Add(input[i+j]);   
                 }

                 var cnt = interval[0].Count();
                 var bufmas = new Double[cnt];
                 for (int j = 0; j < cnt; j++)
                  bufmas[j] = interval.Average(doubles => doubles[j]);

                 interval.Clear();
                 MAs.Add(bufmas);
             }
             return MAs;
        }
    }
}