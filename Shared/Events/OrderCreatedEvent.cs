using MassTransit;
using Shared.Messages;

namespace Shared.Events
{
    public class OrderCreatedEvent : CorrelatedBy<Guid>
    {
        public OrderCreatedEvent(Guid correlationId)
        {
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; set; }
        public List<OrderItemMessage> OrderItems { get; set; }
    }
}