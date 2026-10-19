using ConvenienceStore.Domain.Common;

namespace ConvenienceStore.Domain.Entities.Catalog;

public class ProductBarcode : AuditableEntity<Guid>
{
    public Guid ProductId { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public string BarcodeValue { get; private set; } = null!;
    public bool IsPrimary { get; private set; }

    public Product Product { get; private set; } = default!;
    public UnitOfMeasure UnitOfMeasure { get; private set; } = default!;

    private ProductBarcode() { }
    
    public static ProductBarcode Create(Guid productId, Guid unitOfMeasureId, string barcodeValue, bool isPrimary = false)
    {
        return new ProductBarcode
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UnitOfMeasureId = unitOfMeasureId,
            BarcodeValue = barcodeValue,
            IsPrimary = isPrimary
        };
    }
}
