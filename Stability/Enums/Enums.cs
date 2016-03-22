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
        WIEGHT_CALIBRATE
    };
}