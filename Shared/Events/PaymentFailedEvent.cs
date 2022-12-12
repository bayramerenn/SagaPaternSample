using MassTransit;
using Shared.Messages;

namespace Shared.Events
{
    public class PaymentFailedEvent : CorrelatedBy<Guid>
    {
        public PaymentFailedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; }
        public List<OrderItemMessage> OrderItems { get; set; }
        public string Message { get; set; }
    }
}