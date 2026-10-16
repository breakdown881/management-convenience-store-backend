using ConvenienceStore.Domain.Entities.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConvenienceStore.Infrastructure.Persistence.Configurations.Stores;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(s => s.Code).IsUnique();

        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Address).HasMaxLength(250).IsRequired();
        builder.Property(s => s.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Email).HasMaxLength(100);
        builder.Property(s => s.Region).HasMaxLength(50).IsRequired();

        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasMany(s => s.PosTerminals)
            .WithOne(pt => pt.Store)
            .HasForeignKey(pt => pt.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
