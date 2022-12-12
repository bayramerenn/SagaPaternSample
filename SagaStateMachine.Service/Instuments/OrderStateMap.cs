using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SagaStateMachine.Service.Instuments
{
    public class OrderStateMap : SagaClassMap<OrderStateIntance>
    {
        protected override void Configure(EntityTypeBuilder<OrderStateIntance> entity, ModelBuilder model)
        {
            entity.Property(x => x.BuyerId).IsRequired();
            entity.Property(x => x.OrderId).IsRequired();
            entity.Property(x => x.TotalPrice).HasDefaultValue(0);
        }
    }
}