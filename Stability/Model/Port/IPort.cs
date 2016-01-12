using System;

namespace Stability.Model.Port
{
    public interface IPort
    {
        event EventHandler rx_event;

        bool Connect(out string msg);

        bool Disconnect();
    }
}
