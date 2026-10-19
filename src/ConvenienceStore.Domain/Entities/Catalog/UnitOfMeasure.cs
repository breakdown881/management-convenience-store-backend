using ConvenienceStore.Domain.Common;

namespace ConvenienceStore.Domain.Entities.Catalog;

public class UnitOfMeasure : AuditableEntity<Guid>
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    private UnitOfMeasure() { }
    
    public static UnitOfMeasure Create(string code, string name)
    {
        return new UnitOfMeasure
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim()
        };
    }
}
