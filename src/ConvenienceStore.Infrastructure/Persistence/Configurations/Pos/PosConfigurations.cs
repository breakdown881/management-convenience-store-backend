using ConvenienceStore.Domain.Entities.Pos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConvenienceStore.Infrastructure.Persistence.Configurations.Pos;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ShiftCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(s => s.ShiftCode).IsUnique();

        builder.Property(s => s.OpeningFloat).HasPrecision(18, 2);
        builder.Property(s => s.TotalCashSales).HasPrecision(18, 2);
        builder.Property(s => s.TotalNonCashSales).HasPrecision(18, 2);
        builder.Property(s => s.TotalCashRefunds).HasPrecision(18, 2);
        builder.Property(s => s.TotalPettyCashIn).HasPrecision(18, 2);
        builder.Property(s => s.TotalPettyCashOut).HasPrecision(18, 2);
        builder.Property(s => s.TotalSafeDrops).HasPrecision(18, 2);
        builder.Property(s => s.ExpectedCashAtClose).HasPrecision(18, 2);
        builder.Property(s => s.ActualCountedCashAtClose).HasPrecision(18, 2);
        builder.Property(s => s.CashVariance).HasPrecision(18, 2);

        builder.HasMany(s => s.CashTransactions)
            .WithOne()
            .HasForeignKey(ct => ct.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ShiftCashTransactionConfiguration : IEntityTypeConfiguration<ShiftCashTransaction>
{
    public void Configure(EntityTypeBuilder<ShiftCashTransaction> builder)
    {
        builder.ToTable("ShiftCashTransactions");
        builder.HasKey(ct => ct.Id);
        builder.Property(ct => ct.Amount).HasPrecision(18, 2);
        builder.Property(ct => ct.Reason).HasMaxLength(250).IsRequired();
    }
}

public class PosOrderConfiguration : IEntityTypeConfiguration<PosOrder>
{
    public void Configure(EntityTypeBuilder<PosOrder> builder)
    {
        builder.ToTable("PosOrders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.Property(o => o.SubTotal).HasPrecision(18, 2);
        builder.Property(o => o.TotalVatAmount).HasPrecision(18, 2);
        builder.Property(o => o.GrandTotal).HasPrecision(18, 2);
        builder.Property(o => o.AmountTendered).HasPrecision(18, 2);
        builder.Property(o => o.ChangeAmount).HasPrecision(18, 2);
        builder.Property(o => o.Note).HasMaxLength(250);
        builder.Property(o => o.VoidOrReturnReason).HasMaxLength(250);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.PosOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PosOrderItemConfiguration : IEntityTypeConfiguration<PosOrderItem>
{
    public void Configure(EntityTypeBuilder<PosOrderItem> builder)
    {
        builder.ToTable("PosOrderItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Sku).HasMaxLength(50).IsRequired();
        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Quantity).HasPrecision(18, 3);
        builder.Property(i => i.ConversionFactor).HasPrecision(18, 4);
        builder.Ignore(i => i.BaseUnitQuantity);

        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.LineSubTotal).HasPrecision(18, 2);
        builder.Property(i => i.LineVatAmount).HasPrecision(18, 2);
        builder.Property(i => i.LineTotal).HasPrecision(18, 2);
    }
}
