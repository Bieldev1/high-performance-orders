using Domain.AggregatesModel.PedidoAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EntityConfigurations.PedidoAggregate;

internal class PedidoMap : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.HasKey(it => it.Id);

        builder.Property(it => it.Id).HasColumnName("Id").UseIdentityColumn();
        builder.Property(it => it.ClienteId).HasColumnName("ClienteId").IsRequired();
        builder.Property(it => it.Status).HasColumnName("Status").HasConversion<int>().IsRequired();
        builder.Property(it => it.ValorTotal).HasColumnName("ValorTotal").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(it => it.DataPedido).HasColumnName("DataPedido").IsRequired();

        builder.HasMany(it => it.Itens)
            .WithOne()
            .HasForeignKey(it => it.PedidoId);

        builder.Metadata.FindNavigation(nameof(Pedido.Itens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // Reflete o índice obrigatório da spec (Status, ValorTotal, DataPedido DESC) INCLUDE (ClienteId).
        builder.HasIndex(it => new { it.Status, it.ValorTotal, it.DataPedido })
            .HasDatabaseName("IX_Pedidos_Status_ValorTotal_DataPedido")
            .IsDescending(false, false, true)
            .IncludeProperties(it => it.ClienteId);

        // Covering index para a listagem paginada sem key lookup.
        builder.HasIndex(it => new { it.Status, it.ValorTotal })
            .HasDatabaseName("IX_Pedidos_Covering")
            .IncludeProperties(it => new { it.DataPedido, it.ClienteId, it.Id });
    }
}
