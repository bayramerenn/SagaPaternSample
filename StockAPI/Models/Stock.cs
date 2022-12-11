﻿namespace StockAPI.Models
{
    public class Stock
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Count { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}