using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Modes;
using OrderAPI.Modes.Context;
using OrderAPI.ViewModels;
using Shared.Events;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderController(ApplicationDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
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

            OrderCreatedEvent orderCreatedEvent = new()
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

            await _publishEndpoint.Publish(orderCreatedEvent);

            return Ok(true);
        }
    }
}