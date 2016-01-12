using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stability.Model
{
    interface IDevice
    {
        bool Connect();
        bool Disconnect();
        void Calibrate();
        void StartMeasurement();
        void StopMeasurement();
    }
}
