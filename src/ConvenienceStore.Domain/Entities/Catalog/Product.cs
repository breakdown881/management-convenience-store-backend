using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Enums;

namespace ConvenienceStore.Domain.Entities.Catalog;

public class Product : AuditableEntity<Guid>, ISoftDeletable, IAggregateRoot
{
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public Guid CategoryId { get; private set; }
    public Guid? BrandId { get; private set; }
    public Guid BaseUnitId { get; private set; }

    // Giá bán lẻ tiêu chuẩn (trên đơn vị cơ sở), giá vốn ban đầu & thuế VAT
    public decimal StandardPrice { get; private set; }
    public decimal InitialCostPrice { get; private set; }
    public VatRate VatRate { get; private set; }

    // Cấu hình hàng cân điện tử (Weight-embedded barcode: 20PPPPPWWWWWC)
    public bool IsWeightBased { get; private set; }
    public string? PluCode { get; private set; } // Mã 5 chữ số trên cân điện tử (VD: "00105")

    public bool IsActive { get; private set; }

    // Thực thi ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOnUtc { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation Properties
    public Category Category { get; private set; } = default!;
    public Brand? Brand { get; private set; }
    public UnitOfMeasure BaseUnit { get; private set; } = default!;

    private readonly List<ProductBarcode> _barcodes = new();
    public IReadOnlyCollection<ProductBarcode> Barcodes => _barcodes.AsReadOnly();

    private readonly List<ProductUnitConversion> _unitConversions = new();
    public IReadOnlyCollection<ProductUnitConversion> UnitConversions => _unitConversions.AsReadOnly();

    private Product() { }

    public static Product Create(
        string sku,
        string name,
        Guid categoryId,
        Guid? brandId,
        Guid baseUnitId,
        decimal standardPrice,
        decimal initialCostPrice,
        VatRate vatRate,
        bool isWeightBased = false,
        string? pluCode = null
    )
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            CategoryId = categoryId,
            BrandId = brandId,
            BaseUnitId = baseUnitId,
            StandardPrice = standardPrice,
            InitialCostPrice = initialCostPrice,
            VatRate = vatRate,
            IsWeightBased = isWeightBased,
            PluCode = isWeightBased ? pluCode?.Trim().PadLeft(5, '0') : null,
            IsActive = true,
            IsDeleted = false
        };
    }

    public void AddBarcode(Guid unitOfMeasureId, string barcodeValue, bool isPrimary = false)
    {
        if (_barcodes.Any(b => b.BarcodeValue == barcodeValue.Trim()))
            return;

        _barcodes.Add(ProductBarcode.Create(Id, unitOfMeasureId, barcodeValue, isPrimary));
    }

    public void AddUnitConversion(Guid unitOfMeasureId, decimal conversionFactor, decimal salePrice)
    {
        _unitConversions.Add(ProductUnitConversion.Create(Id, unitOfMeasureId, conversionFactor, salePrice));
    }

    public void UpdatePrice(decimal newStandardPrice, VatRate newVatRate)
    {
        StandardPrice = newStandardPrice;
        VatRate = newVatRate;
    }
}
