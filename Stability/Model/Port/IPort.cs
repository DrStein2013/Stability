using System;

namespace Stability.Model.Port
{
    /// <summary>
    /// Аргумент для события изменения статуса прота
    /// </summary>
    public class PortStatusChangedEventArgs : EventArgs
    {
        public EPortStatus Status { get; set; }
    }

    /// <summary>
    /// Интерфейс, описывающий основные функции портов, используется для абстрагирования от конкретного порта
    /// </summary>
    public interface IPort
    {
        /// <summary>
        /// Событие вычитки из порта, взводится, когда есть что читать.
        /// Привязать на это событие обработчик вычитки данных
        /// </summary>
        /// <remarks></remarks>
        event EventHandler RxEvent;
        /// <summary>
        /// Событие изменения статуса порта, показывает открыт порт, или нет
        /// </summary>
        event EventHandler<PortStatusChangedEventArgs> PortStatusChanged;
        /// <summary>
        /// Метод открытия порта
        /// </summary>
        bool Connect(out string msg);
        /// <summary>
        /// Метод закрытия порта
        /// </summary>
        bool Disconnect();
    }
}
