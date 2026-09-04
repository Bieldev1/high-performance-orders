using Domain.AggregatesModel.PedidoAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EntityConfigurations.PedidoAggregate;

internal class ItemPedidoMap : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ItensPedido");

        builder.HasKey(it => it.Id);

        builder.Property(it => it.Id).HasColumnName("Id").UseIdentityColumn();
        builder.Property(it => it.PedidoId).HasColumnName("PedidoId").IsRequired();
        builder.Property(it => it.NomeProduto).HasColumnName("NomeProduto").HasColumnType("varchar(200)").IsRequired();
        builder.Property(it => it.Quantidade).HasColumnName("Quantidade").IsRequired();
        builder.Property(it => it.PrecoUnitario).HasColumnName("PrecoUnitario").HasColumnType("decimal(18,2)").IsRequired();

        // Reflete o índice obrigatório da spec: IX_OrderItems_OrderId INCLUDE (ProductName, Quantity).
        builder.HasIndex(it => it.PedidoId)
            .HasDatabaseName("IX_ItensPedido_PedidoId")
            .IncludeProperties(it => new { it.NomeProduto, it.Quantidade });
    }
}
