using System;
using Stability.Model;
using Stability.Model.Port;

namespace Stability.View
{
    public enum DeviceCmd
    {
        START_MEASURE = 0,
        STOP_MEASURE,
        ZERO_CALIBRATE
    };

    public class DeviceCmdArgEvent : EventArgs
    {
        public DeviceCmd cmd { get; set; }
    }

    interface IView
    {
        void UpdateTenzView(string[] tenz);
        
        void COnPortStatusChanged(object sender, PortStatusChangedEventArgs portStatusChangedEventArgs);


        event EventHandler ViewUpdated;
        event EventHandler<DeviceCmdArgEvent> DeviceCmdEvent;
    }
}
