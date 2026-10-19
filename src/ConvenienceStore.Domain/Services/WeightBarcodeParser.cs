namespace ConvenienceStore.Domain.Services;

public record WeightBarcodeResult(
    bool IsWeightBarcode,
    string? PluCode,
    decimal WeightInKg
);

/// <summary>
/// Giải mã mã vạch cân điện tử chuẩn GS1-13 (Định dạng: 20PPPPPWWWWWC hoặc 21PPPPPWWWWWC)
/// - 2 ký tự đầu (20..29): Prefix mã vạch cân nội bộ
/// - 5 ký tự tiếp (PPPPP): Mã PLU định danh sản phẩm tươi sống
/// - 5 ký tự tiếp (WWWWW): Trọng lượng tính bằng Gram (chia 1000 ra Kg)
/// - 1 ký tự cuối (C): Checksum EAN-13
/// </summary>

public static class WeightBarcodeParser
{
    public static WeightBarcodeResult TryParse(string rawBarcode)
    {
        if (string.IsNullOrWhiteSpace(rawBarcode))
        {
            return new WeightBarcodeResult(false, null, 0);
        }

        var cleaned = rawBarcode.Trim();
        if (cleaned.Length != 13 || !cleaned.All(char.IsDigit))
            return new WeightBarcodeResult(false, null, 0);

        var prefix = int.Parse(cleaned.Substring(0, 2));
        if (prefix < 20 || prefix > 29)
            return new WeightBarcodeResult(false, null, 0);

        var pluCode = cleaned.Substring(2, 5);
        var weightGrams = int.Parse(cleaned.Substring(7, 5));
        var weightKg = Math.Round(weightGrams / 100m, 3);

        return new WeightBarcodeResult(true, pluCode, weightKg);
    }
}
