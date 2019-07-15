using System;

namespace EquityTrader
{
    /// <summary>
    /// Equity Order that checks PriceThreshhold and places order to  buy for specific equitycode
    /// Once the order is processed it will ignore further prices
    /// </summary>
    public class EquityOrder : IEquityOrder
    {
        private readonly object _tickProcessingLock = new object();
        private readonly IOrderService _orderService;
        private readonly string _equityCode;
        private readonly decimal _priceThreshhold;
        private readonly int _quantity;
        private bool _orderIsActive = true;

        public event OrderPlacedEventHandler OrderPlaced;
        public event OrderErroredEventHandler OrderErrored;

        public EquityOrder(IOrderService orderService, string equityCode, decimal priceThreshhold, int quantity)
        {
            _orderService = orderService;
            _equityCode = equityCode;
            _priceThreshhold = priceThreshhold;
            _quantity = quantity;
        }


        /// <summary>
        /// Get Ticket Price , checkes the prices and places the order
        /// </summary>
        /// <param name="equityCode">Equity Code</param>
        /// <param name="price">Price</param>
        public void ReceiveTick(string equityCode, decimal price)
        {
            //Process only if order is not raised and below threshold price
            if (IsRelevantEquityCode(equityCode) && IsBelowOrderThreshold(price))
            {
               // Tick Processing Lock
                lock (_tickProcessingLock)
                {
                    if (_orderIsActive)
                    {
                        try
                        {
                            _orderService.Buy(equityCode, _quantity, price);
                            OnOrderPlaced(new OrderPlacedEventArgs(equityCode, price));
                        }
                        catch (Exception ex)
                        {
                            OnErrored(new OrderErroredEventArgs(equityCode, price, ex));
                        }
                        finally
                        {
                            _orderIsActive = false;
                        }
                    }
                }
            }
        }

        private bool IsRelevantEquityCode(string equityCode)
        {
            return _equityCode == equityCode;
        }

        private bool IsBelowOrderThreshold(decimal price)
        {
            return price < _priceThreshhold;
        }

        private void OnOrderPlaced(OrderPlacedEventArgs e)
        {
            OrderPlacedEventHandler handler = OrderPlaced;
            handler?.Invoke(e);
        }

        private void OnErrored(OrderErroredEventArgs e)
        {
            OrderErroredEventHandler handler = OrderErrored;
            handler?.Invoke(e);
        }
    }
}
