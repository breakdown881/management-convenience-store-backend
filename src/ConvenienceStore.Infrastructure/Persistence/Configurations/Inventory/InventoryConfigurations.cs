using ConvenienceStore.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConvenienceStore.Infrastructure.Persistence.Configurations.Inventory;

public class InventoryConfigurations : IEntityTypeConfiguration<StoreInventory>
{
    public void Configure(EntityTypeBuilder<StoreInventory> builder)
    {
        builder.ToTable("StoreInventories");
        builder.HasKey(si => si.Id);

        builder.Property(si => si.OnHandQuantity).HasPrecision(18, 3);
        builder.Property(si => si.ReservedQuantity).HasPrecision(18, 3);
        builder.Property(si => si.ReorderPoint).HasPrecision(18, 3);
        builder.Ignore(si => si.AvailableQuantity);

        // Sử dụng xmin của PostgreSQL làm Optimistic Concurrency Token chống bán lố tồn kho
        builder.Property(si => si.Version).IsRowVersion();

        builder.HasIndex(si => new { si.StoreId, si.ProductId }).IsUnique();
    }
}

public class StockCardEntryConfiguration : IEntityTypeConfiguration<StockCardEntry>
{
    public void Configure(EntityTypeBuilder<StockCardEntry> builder)
    {
        builder.ToTable("StockCardEntries");
        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.ReferenceCode).HasMaxLength(50).IsRequired();
        builder.Property(sc => sc.BalanceBefore).HasPrecision(18, 3);
        builder.Property(sc => sc.QuantityChange).HasPrecision(18, 3);
        builder.Property(sc => sc.BalanceAfter).HasPrecision(18, 3);
        builder.Property(sc => sc.Note).HasMaxLength(300);

        builder.HasIndex(sc => new { sc.StoreId, sc.ProductId, sc.CreatedOnUtc });
    }
}

public class StockWriteOffConfiguration : IEntityTypeConfiguration<StockWriteOff>
{
    public void Configure(EntityTypeBuilder<StockWriteOff> builder)
    {
        builder.ToTable("StockWriteOffs");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(w => w.Code).IsUnique();
        builder.Property(w => w.Quantity).HasPrecision(18, 3);
        builder.Property(w => w.Reason).HasMaxLength(300).IsRequired();
    }
}
