using ConvenienceStore.Domain.Common;

namespace ConvenienceStore.Domain.ValueObjects;

public sealed class Barcode : ValueObject
{
    public string Value { get; }

    public Barcode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Barcode value cannot be empty.", nameof(value));


        var trimmed = value.Trim();
        if (trimmed.Length < 3 || trimmed.Length > 32)
            throw new ArgumentException("Barcode length must be between 3 and 32 characters.", nameof(value));

        Value = trimmed;
    }

    private Barcode()
    {
        Value = string.Empty;
    }

    public static Barcode Create(string value) => new(value);

    /// <summary>
    /// Nhận diện mã tem in từ cân điện tử theo chuẩn GS1 (Bắt đầu bằng 20, 21, 22).
    /// </summary>
    public bool IsWeightEmbeddedBarcode() => Value.Length == 13 && (Value.StartsWith("20") || Value.StartsWith("21") || Value.StartsWith("22"));

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}