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
        private readonly ISendEndpoint _senderEndpoint;

        public OrderCreateEventConsumer(ApplicationDbContext context, ISendEndpointProvider sendEndpointProvider = null)
        {
            _context = context;
            _sendEndpointProvider = sendEndpointProvider;
            _senderEndpoint =  _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachine}")).Result; //deneyecegiz
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachine}"));

            foreach (var orderItem in context.Message.OrderItems)
            {
                var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.ProductId == orderItem.ProductId && s.Count > orderItem.Count);

                if (stock != null)
                {
                    stock.Count -= orderItem.Count;
                    stock.CreatedDate = DateTime.UtcNow;

                    //_context.Stocks.Update(stock);
                    await _context.SaveChangesAsync();


                    StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
                    {
                        OrderItems = context.Message.OrderItems
                    };

                    await sendEndpoint.Send(stockReservedEvent);
                    Console.WriteLine("Payment_StockReservedEventQueue tetiklendi.");
                }
                else
                {

                    StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
                    {
                        Message = "Stock miktari yetersiz"
                    };

                    await sendEndpoint.Send(stockNotReservedEvent);
                    Console.WriteLine("Order_StockNotReservedEventQueue tetiklendi.");
                }
            }

            
        }
    }
}