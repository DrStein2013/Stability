using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stability.Model
{
    interface IDevice
    {
        void Calibrate();
        void StartMeasurement();
        void StopMeasurement();
    }
}
