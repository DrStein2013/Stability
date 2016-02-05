using System;
using Stability.Model;
using Stability.Model.Port;

namespace Stability.View
{
    public enum DeviceCmd
    {
        START_MEASURE = 0,
        STOP_MEASURE,
        ZERO_CALIBRATE,
        WIEGHT_CALIBRATE
    };

    public class CalibrationParams
    {
        public byte TenzNumber { get; set; }
        public double Weight { get; set; }
        public int EntryCount { get; set; }
        public int Period { get; set; }
    }
    public class DeviceCmdArgEvent : EventArgs
    {
        public DeviceCmd cmd { get; set; }
        public CalibrationParams Params { get; set; }
    }

    public interface IView
    {
        void UpdateTenzView(string[] tenz);
        
        void COnPortStatusChanged(object sender, PortStatusChangedEventArgs portStatusChangedEventArgs);


        event EventHandler ViewUpdated;
        event EventHandler<DeviceCmdArgEvent> DeviceCmdEvent;
    }
}
