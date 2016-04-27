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
        Add = 0,
        Find,
        Delete,
        Update
    }
}