using MassTransit;
using Shared;
using Shared.Events;

namespace PaymentAPI.Consumers
{
    public class PaymentStartedEventConsumer : IConsumer<PaymentStartedEvent>
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public PaymentStartedEventConsumer(ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<PaymentStartedEvent> context)
        {
            //process
            ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachine}"));

            bool paymentState = true;

            if (paymentState)
            {
                Console.WriteLine("Ödeme başarılı...");

                PaymentCompletedEvent paymentCompletedEvent = new(context.Message.CorrelationId);

                await sendEndpoint.Send(paymentCompletedEvent);
            }
            else
            {
                Console.WriteLine("Ödeme başarısız...");
                PaymentFailedEvent paymentFailedEvent = new(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems,
                    Message = "Bakiye yetersiz!"
                };
                await sendEndpoint.Send(paymentFailedEvent);
            }
        }
    }
}