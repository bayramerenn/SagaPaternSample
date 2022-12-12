using MassTransit;
using OrderAPI.Modes.Context;
using Shared.Events;

namespace OrderAPI.Consumer
{
    public class OrderCompletedEventConsumer : IConsumer<OrderCompletedEvent>
    {
        private readonly ApplicationDbContext _context;

        public OrderCompletedEventConsumer(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            var order = await _context.Orders.FindAsync(context.Message.OrderId);

            if (order != null)
            {
                order.OrderStatus = Modes.Enums.OrderStatus.Completed;
                await _context.SaveChangesAsync();
                Console.WriteLine("Islem completed alinmistir");
            }
        }
    }
}