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
               case FilterType.MovingMedian:
                    return MovingMedian(fltParams[0], input);
            }
            return null;
        }


        private List<double[]> MovingAverage(int window,List<double[]> input)
        {
            var interval = new List<double[]>(window);
            List<double[]> MAs = new List<double[]>();
            int winmod = 0;
            for (int i = 0; i < input.Count; i++)
            {
                if (i > input.Count - window)
                    winmod++;//window--;
                
                 for (int j = 0; j < window; j++)
                 {
                     interval.Add(input[i+j-winmod]);   
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

        private List<double[]> MovingMedian(int window, List<double[]> input)
        {
            var cnt = input[0].Count();
            var interval = new List<double>();
            
            List<double[]> MAs = new List<double[]>();

            for (int i = 0; i < input.Count; i++)   //цикл по всему списку
            {
                var median_entry = new double[cnt];
                if (i > input.Count - window)
                    window--; //winmod++

                for (int k = 0; k < cnt; k++) //цикл по количеству записей в одном элементе списка
                {
                    interval.Clear();
                    for (int j = 0; j < window; j++) //цикл по окну
                    {
                        interval.Add(input[i + j][k]);
                    }
                    interval.Sort();
                    if (window%2 == 0)
                        median_entry[k] = (interval[window/2 - 1] + interval[window/2])/2;
                    else median_entry[k] = interval[window/2];
                }

                MAs.Add(median_entry);
            }
            return MAs;
        }
    }
}