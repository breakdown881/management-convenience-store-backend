namespace ConvenienceStore.Application.Common.Interfaces;

public record BarcodeLookupResultDto(
    Guid ProductId,
    string Sku,
    string ProductName,
    Guid UnitOfMeasureId,
    string UnitName,
    string ScannedBarcode,
    decimal Qauntity,
    decimal ConversionFactor,
    decimal UnitPrice,
    int VatPercent,
    decimal LineTotal,
    bool IsWeightBased
);

public interface IBarcodeLookupService
{
    Task<BarcodeLookupResultDto?> LookupAsync(string rawBarcode, CancellationToken cancellationToken = default);
}
