using ConvenienceStore.Domain.Common;

namespace ConvenienceStore.Domain.Entities.Catalog;

public class Brand : AuditableEntity<Guid>
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? CountryOfOrigin { get; private set; }

    private Brand() { }
    
    public static Brand Create(string code, string name, string? countryOfOrigin = null)
    {
        return new Brand
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            CountryOfOrigin = countryOfOrigin?.Trim()
        };
    }
}
