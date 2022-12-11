using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using StockAPI.Models;

namespace StockAPI.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly ApplicationDbContext _context;

        public PaymentFailedEventConsumer(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            foreach (var orderItem in context.Message.OrderItems)
            {
                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProductId == orderItem.ProductId);

                if (stock != null)
                {
                    stock.Count += orderItem.Count;
                    await _context.SaveChangesAsync();
                }

            }

        }
    }
}
