using ConvenienceStore.Domain.Common;

namespace ConvenienceStore.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public const string DefaultCurrency = "VND";

    public Money(decimal amount, string currency = DefaultCurrency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Current code cannot be empty", nameof(currency));

        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpperInvariant();
    }

    private Money()
    {
        Amount = 0m;
        Currency = DefaultCurrency;
    }

    public static Money Zero(string currency = DefaultCurrency) => new(0m, currency);
    public static Money From(decimal amount, string currency = DefaultCurrency) => new(amount, currency);

    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator *(Money money, decimal multiplier) => new(money.Amount * multiplier, money.Currency);
    public static Money operator *(Money money, int quantity) => new(money.Amount * quantity, money.Currency);

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException($"Cannot perform operations on different currencies: '{left.Currency}' vs '{right.Currency}'.");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N0} {Currency}";
}