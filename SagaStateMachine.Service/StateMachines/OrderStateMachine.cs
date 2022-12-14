using MassTransit;
using SagaStateMachine.Service.Instuments;
using Shared;
using Shared.Events;
using Shared.Messages;

namespace SagaStateMachine.Service.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderStateIntance>
    {
        public Event<OrderStartedEvent> OrderStartedEvent { get; private set; }
        public Event<StockReservedEvent> StockReservedEvent { get; private set; }
        public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; private set; }
        public Event<PaymentFailedEvent> PaymentFailedEvent { get; private set; }
        public Event<StockNotReservedEvent> StockNotReservedEvent { get; private set; }



        public State OrderCreated { get; private set; }
        public State StockReserved { get; private set; }
        public State PaymentCompleted { get; private set; }
        public State PaymentFailed { get; private set; }
        public State StockNotReserved { get; private set; }

        public OrderStateMachine()
        {
            InstanceState(instance => instance.CurrentState);

            Event(() => OrderStartedEvent,
               orderStateInstance =>
               orderStateInstance.CorrelateBy((instance, context) => instance.OrderId == context.Message.OrderId)
               .SelectId(e => e.CorrelationId ?? Guid.NewGuid()));

            Event(() => StockReservedEvent,
               orderStateInstance =>
               orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(() => StockNotReservedEvent,
               orderStateInstance =>
               orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(() => PaymentCompletedEvent,
               orderStateInstance =>
               orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Event(() => PaymentFailedEvent,
               orderStateInstance =>
               orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

            Initially(
                When(OrderStartedEvent)
                    .Then(context =>
                    {
                        context.Saga.OrderId = context.Message.OrderId;
                        context.Saga.BuyerId = context.Message.BuyerId;
                        context.Saga.TotalPrice = context.Message.TotalPrice;
                        context.Saga.CreatedDate = DateTime.Now;
                    })
                    .Then(context => Console.WriteLine("Process manupelation"))
                    .TransitionTo(OrderCreated)
                    .Then(context => Console.WriteLine("Process"))
                    .Send(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEventQueue}")
                        , context => new OrderCreatedEvent(context.Saga.CorrelationId)
                        {
                            OrderItems = context.Message.OrderItems
                        })
                );

            During(OrderCreated,
                When(StockReservedEvent)
                    .TransitionTo(StockReserved)
                    .Send(new Uri($"queue:{RabbitMQSettings.Payment_StartedEventQueue}")
                            , context => new PaymentStartedEvent(context.Message.CorrelationId)
                            {
                                OrderItems = context.Message.OrderItems,
                                TotalPrice = context.Saga.TotalPrice
                            }),
                When(StockNotReservedEvent)
                    .TransitionTo(StockNotReserved)
                    .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}")
                            , context => new OrderFailedEvent()
                            {
                                OrderId = context.Saga.OrderId,
                                Message = context.Message.Message
                            })
                );

            During(StockReserved,
                When(PaymentCompletedEvent)
                    .TransitionTo(PaymentCompleted)
                    .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderCompletedEventQueue}")
                            , context => new OrderCompletedEvent()
                            {
                                OrderId = context.Saga.OrderId
                            })
                    .Finalize(),
                When(PaymentFailedEvent)
                    .TransitionTo(PaymentFailed)
                    .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}")
                            , context => new OrderFailedEvent()
                            {
                                OrderId = context.Saga.OrderId,
                                Message = context.Message.Message
                            })
                    .Send(new Uri($"queue:{RabbitMQSettings.Stock_RollbackMessageQueue}")
                            , context => new StockRollBackMessage()
                            {
                                OrderItems = context.Message.OrderItems,
                            })
                );

            SetCompletedWhenFinalized();
        }
    }
}