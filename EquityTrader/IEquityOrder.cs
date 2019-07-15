using System;
namespace EquityTrader
{
    public interface IEquityOrder : IOrderPlaced, IOrderErrored
    {
        void ReceiveTick(string equityCode, decimal price);
    }
}
