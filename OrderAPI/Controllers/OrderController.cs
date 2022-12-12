using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Modes;
using OrderAPI.Modes.Context;
using OrderAPI.ViewModels;
using Shared;
using Shared.Events;
using Shared.Messages;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public OrderController(ApplicationDbContext context, ISendEndpointProvider sendEndpointProvider)
        {
            _context = context;
            _sendEndpointProvider = sendEndpointProvider;
        }

        [HttpPost]
        public async Task<IActionResult> Post(OrderVM orderVM)
        {
            Order order = new()
            {
                BuyerId = orderVM.BuyerId,
                CreatedDate = DateTime.Now,
                OrderStatus = Modes.Enums.OrderStatus.Suspend,
                TotalPrice = orderVM.OrderItems.Sum(x => x.Price),
                OrderItems = orderVM.OrderItems.Select(oi => new OrderItem
                {
                    Count = oi.Count,
                    Price = oi.Price,
                    ProductId = oi.ProductId,
                }).ToList()
            };

            await _context.Orders.AddAsync(order);

            await _context.SaveChangesAsync();

            OrderStartedEvent orderStartedEvent = new()
            {
                OrderId = order.Id,
                BuyerId = order.BuyerId,
                TotalPrice = order.TotalPrice,
                OrderItems = order.OrderItems.Select(oi => new OrderItemMessage
                {
                    Price = oi.Price,
                    Count = oi.Count,
                    ProductId = oi.ProductId
                }).ToList()
            };

            ISendEndpoint sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachine}"));
            await sendEndpoint.Send(orderStartedEvent);
            return Ok(true);
        }
    }
}