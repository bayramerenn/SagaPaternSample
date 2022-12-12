using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Messages;
using StockAPI.Models;

namespace StockAPI.Consumers
{
    public class StockRollbackMessageConsumer : IConsumer<StockRollBackMessage>
    {
        private readonly ApplicationDbContext _context;

        public StockRollbackMessageConsumer(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<StockRollBackMessage> context)
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
