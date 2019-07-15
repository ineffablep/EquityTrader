using System;
using NUnit.Framework;
using Moq;
using FluentAssertions;
using System.Threading.Tasks;
using System.Threading;

namespace EquityTrader.Test
{
    public class EquityOrderTest
    {
        public class EquityOrderTests
        {
            const string equityCode = "IBM";
            const decimal orderThreshhold = 10m;
            const decimal priceBelowThreshhold = orderThreshhold - 0.01m;
            const int quantity = 10;
            EquityOrder _equityOrder;
            Mock<IOrderService> _orderServiceMock;

            [SetUp]
            public void Setup()
            {
                _orderServiceMock = new Mock<IOrderService>();
                _equityOrder = new EquityOrder(_orderServiceMock.Object, equityCode, orderThreshhold, quantity);
            }

            [Test]
            public void ReceiveTickShouldPlaceOrderForMatchedTickThresholdPrice()
            {
                _equityOrder.ReceiveTick(equityCode, priceBelowThreshhold);

                _orderServiceMock.Verify(x => x.Buy(equityCode, quantity, priceBelowThreshhold));
                _orderServiceMock.VerifyNoOtherCalls();
            }

            [Test]
            public void ReceiveTickShouldNotPlaceOrderForUnMatchedTick()
            {
                _equityOrder.ReceiveTick("MSFT", priceBelowThreshhold);

                _orderServiceMock.VerifyNoOtherCalls();
            }

            [TestCase(10)]
            [TestCase(10.01)]
            public void ReceiveTickShouldNotPlaceOrderIfPriceIsNotBelowThreshold(decimal price)
            {
                _equityOrder.ReceiveTick(equityCode, price);

                _orderServiceMock.VerifyNoOtherCalls();
            }

            [Test]
            public void ReceiveTickShouldRaiseOrderPlacedEventWhenOrderRaised()
            {
                var eventFired = false;
                _equityOrder.OrderPlaced += (e) =>
                {
                    eventFired = true;
                    e.EquityCode.Should().Be(equityCode);
                    e.Price.Should().Be(priceBelowThreshhold);
                };

                _equityOrder.ReceiveTick(equityCode, priceBelowThreshhold);

                eventFired.Should().BeTrue();
            }

            [Test]
            public void ReceiveTickShouldNotRaiseErrorEventWhenOrderIsSuccess()
            {
                var eventFired = false;
                _equityOrder.OrderErrored += (e) => eventFired = true;

                _equityOrder.ReceiveTick(equityCode, priceBelowThreshhold);

                eventFired.Should().BeFalse();
            }

            [Test]
            public void ReceiveTickShouldRaiseErrorEventWhenExceptionIsThrown()
            {
                var exception = new Exception();
                var eventFired = false;
                _equityOrder.OrderErrored += (e) =>
                {
                    eventFired = true;
                    e.EquityCode.Should().Be(equityCode);
                    e.Price.Should().Be(priceBelowThreshhold);
                    e.GetException().Should().Be(exception);
                };

                _orderServiceMock.Setup(m => m.Buy(equityCode, quantity, priceBelowThreshhold)).Throws(exception);

                _equityOrder.ReceiveTick(equityCode, priceBelowThreshhold);

                eventFired.Should().BeTrue();
            }

            [Test]
            public void ReceiveTickShouldRaiseOnlyOneOrder()
            {
                _equityOrder.ReceiveTick(equityCode, priceBelowThreshhold);
                _orderServiceMock.Verify(x => x.Buy(equityCode, quantity, priceBelowThreshhold));
                _equityOrder.ReceiveTick(equityCode, priceBelowThreshhold);
                _orderServiceMock.VerifyNoOtherCalls();
            }

            [Test]
            public void ReceiveTickShouldStopAfterError()
            {
                var exception = new Exception();
                _orderServiceMock.Setup(m => m.Buy(equityCode, quantity, priceBelowThreshhold)).Throws(exception);
                _equityOrder.ReceiveTick(equityCode, priceBelowThreshhold);
                _equityOrder.ReceiveTick(equityCode, priceBelowThreshhold);

                _orderServiceMock.Verify(m => m.Buy(equityCode, quantity, priceBelowThreshhold), Times.Once);
            }

            [Test]
            public void ReceiveTickShouldOnlyProcessOneOrderForMultipleTrheads()
            {
                _equityOrder.OrderPlaced += (e) =>
                {
                    Console.WriteLine($"EquityCode: {e.EquityCode}, Price: {e.Price}");
                };

                for (int i = 0; i < 20; i++)
                {
                    Task.Factory.StartNew(() =>
                    {
                        var price = orderThreshhold - (decimal)Math.Round(new Random().NextDouble(), 2);
                        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} price {price} start");
                        _equityOrder.ReceiveTick(equityCode, price);
                        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} price {price} finish");
                    });
                }

                _orderServiceMock.Verify(m => m.Buy(equityCode, quantity, It.IsAny<decimal>()), Times.Once);
            }
        }
    }
}
