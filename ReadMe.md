## Task ##
Implement an equity order component in C#

Requirements
1. Build a concrete implementation of the IEquityOrder
2. It will receive all ticks (price updates for equities) from an external tick source via the
ReceiveTick method
3. When a (relevant) tick is received whose price is below a threshold level, the component
should then:
a. Place a buy order via the IOrderService interface
b. Signal the Order Placed Event Handler
c. Shut down - ignoring all further ticks
4. Any errors experienced should cause the component to signal the Order Errored Event
Handler, and then shut down - ignoring all further ticks
5. Each instance of your component should only ever place one order. There may be several
instances active simultaneously
Principles:
1. Requirements must be met.
2. Write the code as you would write a part of a production grade system.
