using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Events;
using StockAPI.Models;

namespace StockAPI.Consumers
{
    public class OrderCreateEventConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly ApplicationDbContext _context;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public OrderCreateEventConsumer(ApplicationDbContext context, ISendEndpointProvider sendEndpointProvider = null)
        {
            _context = context;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            foreach (var orderItem in context.Message.OrderItems)
            {
                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProductId == orderItem.ProductId && s.Count > orderItem.Count);

                if (stock != null)
                {
                    stock.Count -= orderItem.Count;
                    stock.CreatedDate = DateTime.UtcNow;

                    _context.Stocks.Update(stock);

                    ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventQueue}"));

                    StockReservedEvent stockReservedEvent = new()
                    {
                        BuyerId = context.Message.BuyerId,
                        OrderId = context.Message.OrderId,
                        OrderItems = context.Message.OrderItems,
                        TotalPrice = context.Message.TotalPrice
                    };

                    await sendEndpoint.Send(stockReservedEvent);
                    Console.WriteLine("Payment_StockReservedEventQueue tetiklendi.");
                }
                else
                {
                    ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Order_StockNotReservedEventQueue}"));

                    StockNotReservedEvent stockNotReservedEvent = new()
                    {
                        OrderId = context.Message.OrderId,
                        Message = "Stock miktari yetersiz"
                    };

                    await sendEndpoint.Send(stockNotReservedEvent);
                    Console.WriteLine("Order_StockNotReservedEventQueue tetiklendi.");
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}