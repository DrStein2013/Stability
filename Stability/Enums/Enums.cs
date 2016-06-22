namespace Stability.Enums
{
    public enum InputFilterType
    {
        NoFilter=0,AlphaBeta,MovingAverage
    }

    enum StabilityParseMode
    {
        ParseData = 0, ParseCmd
    }

    public enum DeviceCmd
    {
        START_MEASURE = 0,
        STOP_MEASURE,
        START_RECORDING,
        STOP_RECORDING,
        ZERO_CALIBRATE,
        STARTUP_CALIBRATE,
        WEIGHT_CALIBRATE,
        WEIGHT_CALIBRATE_FAST,
        WEIGHT_MEASURE
    };

    public enum Sexes
    {
        Female = 0, Male
    }

    public enum BaseAction
    {
        AddPatient = 0,
        Find,
        Delete,
        Update,
        AddNewEntry,
        ClearEntry
    }

    public enum BaseEntryState
    {
        Empty = 0,
        New,
        Loaded,
        Modified
    }

    public enum FilterType
    {
        MovingAverage=0,
        MovingMedian
    }

    public enum AnalyzerCmd
    {
        ResetAll=0,
        SetTenzos,
        ApplyFilter,
        CalcStabilograms,
        CalcFFT,
        CalcKoefs
    }
}