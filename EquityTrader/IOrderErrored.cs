using System;
namespace EquityTrader
{
    public interface IOrderErrored
    {
        event OrderErroredEventHandler OrderErrored;
    }
}
