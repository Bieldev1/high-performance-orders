using Domain.AggregatesModel.ClienteAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.EntityConfigurations.ClienteAggregate;

internal class ClienteMap : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(it => it.Id);

        builder.Property(it => it.Id).HasColumnName("Id").UseIdentityColumn();
        builder.Property(it => it.Nome).HasColumnName("Nome").HasColumnType("varchar(150)").IsRequired();
        builder.Property(it => it.Email).HasColumnName("Email").HasColumnType("varchar(200)").IsRequired();
        builder.Property(it => it.DataCadastro).HasColumnName("DataCadastro").IsRequired();
    }
}
