using Microsoft.EntityFrameworkCore;

namespace StockAPI.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Stock> Stocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Stock>(builder =>
            {
                builder.HasData(new Stock[]
                {
                    new Stock
                    {
                        Id = 1,
                        Count = 100,
                        CreatedDate = DateTime.Now,
                        ProductId = 1
                    },
                    new Stock
                    {
                        Id = 2,
                        Count = 100,
                        CreatedDate = DateTime.Now,
                        ProductId = 2
                    },
                    new Stock
                    {
                        Id = 3,
                        Count = 100,
                        CreatedDate = DateTime.Now,
                        ProductId = 3
                    },
                    new Stock
                    {
                        Id = 4,
                        Count = 100,
                        CreatedDate = DateTime.Now,
                        ProductId = 4
                    },
                    new Stock
                    {
                        Id = 5,
                        Count = 100,
                        CreatedDate = DateTime.Now,
                        ProductId = 5
                    }
                });
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}