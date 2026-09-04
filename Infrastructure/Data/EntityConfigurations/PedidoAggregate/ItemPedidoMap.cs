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

        // Índice de performance criado via SQL puro em database/scripts/01-create-tables.sql, não aqui.
    }
}
