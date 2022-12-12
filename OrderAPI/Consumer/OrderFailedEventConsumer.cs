using MassTransit;
using OrderAPI.Modes.Context;
using Shared.Events;

namespace OrderAPI.Consumer
{
    public class OrderFailedEventConsumer : IConsumer<OrderFailedEvent>
    {
        private readonly ApplicationDbContext _context;

        public OrderFailedEventConsumer(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<OrderFailedEvent> context)
        {
            var order = await _context.Orders.FindAsync(context.Message.OrderId);

            if (order != null)
            {
                order.OrderStatus = Modes.Enums.OrderStatus.Fail;
                await _context.SaveChangesAsync();
                Console.WriteLine(context.Message.Message);
            }
        }
    }
}