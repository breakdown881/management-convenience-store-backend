using System.Text.Json;
using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Services;
using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ConvenienceStore.Infrastructure.Services;

public class FastBarcodeLookupService : IBarcodeLookupService
{
    private readonly string _connectionString;
    private readonly IDistributedCache _cache;

    public FastBarcodeLookupService(IConfiguration configuration, IDistributedCache cache)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _cache = cache;
    }

    public async Task<BarcodeLookupResultDto?> LookupAsync(string rawBarcode, CancellationToken cancellationToken = default)
    {
        var cleaned = rawBarcode.Trim();

        // 1. Kiểm tra xem có phải Mã vạch cân điện tử GS1-13 (20PPPPPWWWWWC) hay không
        var weightParse = WeightBarcodeParser.TryParse(cleaned);
        if (weightParse.IsWeightBarcode && weightParse.PluCode != null)
        {
            return await LookupWeightProductAsync(cleaned, weightParse.PluCode, weightParse.WeightInKg, cancellationToken);
        }

        // 2. Tra cứu từ Redis Cache trước (Tốc độ < 2ms)
        var cacheKey = $"pos:barcode:{cleaned}";
        var cachedJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            return JsonSerializer.Deserialize<BarcodeLookupResultDto>(cachedJson);
        }

        // 3. Nếu Cache Miss -> Truy vấn phẳng siêu tốc qua Dapper
        const string sql = """
            SELECT
                p."Id" AS ProductId,
                p."Sku" AS Sku,
                p."Name" AS ProductName,
                pb."UnitOfMeasureId" AS UnitOfMeasureId,
                u."Name" AS UnitName,
                pb."BarcodeValue" AS ScannedBarcode,
                COALESCE(pub."ConversionFactor", 1.0) AS ConversionFactor,
                COALESCE(puc."SalePrice", p."StandardPrice") AS UnitPrice,
                p."VatRate" AS VatPercent,
                p."IsWeightBased" AS IsWeightBased
            FROM "ProductBarcodes" pb
            INNER JOIN "Products" p ON p."Id" = pb."ProductId" AND p."IsDeleted" = FALSE AND p."IsActive" = TRUE
            INNER JOIN "UnitsOfMeasure" u ON u."Id" = pb."UnitOfMeasureId"
            LEFT JOIN "ProductUnitConversions" puc ON puc."ProductId" = p."Id" AND puc."UnitOfMeasuredId" = pb."UnitOfMeasureId"
            WHERE pb."BarcodeValue" = @Barcode
            LIMIT 1;
        """;

        await using var connection = new NpgsqlConnection(_connectionString);
        var row = await connection.QueryFirstOrDefaultAsync<RawBarcodeRow>(sql, new { Barcode = cleaned });
        if (row == null)
            return null;

        var lineSubTotal = Math.Round(1m * row.UnitPrice, 0);
        var lineVat = Math.Round(lineSubTotal * (row.VatPercent / 100m), 0);

        var dto = new BarcodeLookupResultDto(
            row.ProductId,
            row.Sku,
            row.ProductName,
            row.UnitOfMeasureId,
            row.UnitName,
            row.ScannedBarcode,
            1m,
            row.ConversionFactor,
            row.UnitPrice,
            row.VatPercent,
            lineSubTotal + lineVat,
            row.IsWeightBased
        );

        // Lưu vào Redis Cache trong 30 phút
        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(dto),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) },
            cancellationToken
        );

        return dto;
    }

    private async Task<BarcodeLookupResultDto?> LookupWeightProductAsync(
        string scannedBarcode,
        string pluCode,
        decimal weightInKg,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            SELECT 
                p."Id" AS ProductId,
                p."Sku" AS Sku,
                p."Name" AS ProductName,
                p."BaseUnitId" AS UnitOfMeasureId,
                u."Name" AS UnitName,
                p."StandardPrice" AS UnitPrice,
                p."VatRate" AS VatPercent
            FROM "Products" p
            INNER JOIN "UnitsOfMeasure" u ON u."Id" = p."BaseUnitId"
            WHERE p."IsWeightBased" = TRUE AND p."PluCode" = @PluCode AND p."IsDeleted" = FALSE AND p."IsActive" = TRUE
            LIMIT 1;
        """;

        await using var connection = new NpgsqlConnection(_connectionString);
        var row = await connection.QueryFirstOrDefaultAsync<RawBarcodeRow>(sql, new { PluCode = pluCode });
        if (row == null)
            return null;

        var subTotal = Math.Round(weightInKg * row.UnitPrice, 0);
        var vat = Math.Round(subTotal * (row.VatPercent / 100m), 0);

        return new BarcodeLookupResultDto(
            row.ProductId,
            row.Sku,
            row.ProductName,
            row.UnitOfMeasureId,
            row.UnitName,
            scannedBarcode,
            weightInKg,
            1m,
            row.UnitPrice,
            row.VatPercent,
            subTotal + vat,
            true
        );
    }

    private sealed class RawBarcodeRow
    {
        public Guid ProductId { get; set; }
        public string Sku { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public Guid UnitOfMeasureId { get; set; }
        public string UnitName { get; set; } = null!;
        public string ScannedBarcode { get; set; } = null!;
        public decimal ConversionFactor { get; set; }
        public decimal UnitPrice { get; set; }
        public int VatPercent { get; set; }
        public bool IsWeightBased { get; set; }
    }
}
