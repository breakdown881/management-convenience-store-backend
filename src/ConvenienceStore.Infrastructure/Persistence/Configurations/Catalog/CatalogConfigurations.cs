using ConvenienceStore.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConvenienceStore.Infrastructure.Persistence.Configurations.Catalog;

public class CatalogConfigurations : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).HasMaxLength(30).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
    }
}

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Code).HasMaxLength(30).IsRequired();
        builder.HasIndex(b => b.Code).IsUnique();
        builder.Property(b => b.Name).HasMaxLength(100).IsRequired();
    }
}

public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("UnitsOfMeasure");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(u => u.Code).IsUnique();
        builder.Property(u => u.Name).HasMaxLength(50).IsRequired();
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.StandardPrice).HasPrecision(18, 2);
        builder.Property(p => p.InitialCostPrice).HasPrecision(18, 2);
        builder.Property(p => p.PluCode).HasMaxLength(10);
        builder.HasIndex(p => p.PluCode);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Brand)
            .WithMany()
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.BaseUnit)
            .WithMany()
            .HasForeignKey(p => p.BaseUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Barcodes)
            .WithOne(b => b.Product)
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.UnitConversions)
            .WithOne(uc => uc.Product)
            .HasForeignKey(uc => uc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductBarcodeConfiguration : IEntityTypeConfiguration<ProductBarcode>
{
    public void Configure(EntityTypeBuilder<ProductBarcode> builder)
    {
        builder.ToTable("ProductBarcodes");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BarcodeValue).HasMaxLength(50).IsRequired();
        builder.HasIndex(b => b.BarcodeValue).IsUnique();
    }
}

public class ProductUnitConversionConfiguration : IEntityTypeConfiguration<ProductUnitConversion>
{
    public void Configure(EntityTypeBuilder<ProductUnitConversion> builder)
    {
        builder.ToTable("ProductUnitConversions");
        builder.HasKey(uc => uc.Id);
        builder.Property(uc => uc.ConversionFactor).HasPrecision(18, 4);
        builder.Property(uc => uc.SalePrice).HasPrecision(18, 2);
        builder.HasIndex(uc => new { uc.ProductId, uc.UnitOfMeasureId }).IsUnique();
    }
}