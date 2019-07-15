using System;
namespace EquityTrader
{
    public interface IOrderPlaced
    {
        event OrderPlacedEventHandler OrderPlaced;
    }
}
