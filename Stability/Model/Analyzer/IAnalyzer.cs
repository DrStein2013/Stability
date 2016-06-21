using Stability.Enums;

namespace Stability.Model.Analyzer
{
    public interface IAnalyzer
    {
       // void AverageFilter();
       // void MovingAverageFilter();
        void Filter(FilterType type);
        void FFT();
    }
}