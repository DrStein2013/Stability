using System;
using Stability.Enums;
using Stability.Model;
using Stability.Model.Device;
using Stability.Model.Port;

namespace Stability.View
{
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
        public int MeasureTime { get; set; }
        public CalibrationParams Params { get; set; }
    }

    public interface IView
    {
        void UpdateTenzView(string[] tenz);
        void COnPortStatusChanged(object sender, PortStatusChangedEventArgs portStatusChangedEventArgs);
        void UpdatePatientData(PatientModelResponseArg patientModelResponseArg);
        void UpdateAnamnesisData(AnamnesisModelResponseArg anamnesisModelResponseArg);
        void UpdateDataInGridRes(DeviceDataEntry d);
        void UpdateButtons();
        void UpdateProgress(ProgressEventArgs progress);

        event EventHandler ViewUpdated;
        event EventHandler<DeviceCmdArgEvent> DeviceCmdEvent;
        event EventHandler<PatientModelResponseArg> PatientEvent;
        event EventHandler<AnamnesisModelResponseArg> AnamnesisEvent;
    }
}
