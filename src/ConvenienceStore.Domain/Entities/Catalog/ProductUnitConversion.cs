using ConvenienceStore.Domain.Common;

namespace ConvenienceStore.Domain.Entities.Catalog;

/// <summary>
/// Quy đổi đơn vị tính đa cấp (VD: 1 Lốc = 6 Lon, 1 Thùng = 24 Lon) kèm giá bán theo đơn vị quy đổi
/// </summary>
public class ProductUnitConversion : AuditableEntity<Guid>
{
    public Guid ProductId { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal ConversionFactor { get; private set; }
    public decimal SalePrice { get; private set; }

    public Product Product { get; private set; } = default!;
    public UnitOfMeasure UnitOfMeasure { get; private set; } = default!;

    private ProductUnitConversion() { }
    
    public static ProductUnitConversion Create(
        Guid productId,
        Guid unitOfMeasureId,
        decimal conversionFactor,
        decimal salePrice
    )
    {
        if (conversionFactor <= 0)
        {
            throw new ArgumentException("Hệ số quy đổi phải lớn hơn 0.", nameof(conversionFactor));
        }

        return new ProductUnitConversion
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UnitOfMeasureId = unitOfMeasureId,
            ConversionFactor = conversionFactor,
            SalePrice = salePrice
        };
    }
}
