using ConvenienceStore.Domain.Common;

namespace ConvenienceStore.Domain.Entities.Catalog;

public class Category : AuditableEntity<Guid>
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public Guid? ParentId { get; private set; }
    public bool IsActive { get; private set; }

    private Category() { }
    
    public static Category Create(string code, string name, Guid? parentId = null)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            ParentId = parentId,
            IsActive = true
        };
    }
}
