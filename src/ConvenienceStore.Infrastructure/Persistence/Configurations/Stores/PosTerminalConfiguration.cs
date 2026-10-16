using ConvenienceStore.Domain.Entities.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConvenienceStore.Infrastructure.Persistence.Configurations.Stores;

public class PosTerminalConfiguration : IEntityTypeConfiguration<PosTerminal>
{
    public void Configure(EntityTypeBuilder<PosTerminal> builder)
    {
        builder.ToTable("PosTerminals");
        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.TerminalCode).HasMaxLength(20).IsRequired();
        builder.Property(pt => pt.Name).HasMaxLength(100).IsRequired();
        builder.Property(pt => pt.DeviceFingerprint).HasMaxLength(255);
        builder.Property(pt => pt.IpAddress).HasMaxLength(50);

        // Mỗi cửa hàng không được trùng mã quầy
        builder.HasIndex(pt => new { pt.StoreId, pt.TerminalCode }).IsUnique();
    }
}
