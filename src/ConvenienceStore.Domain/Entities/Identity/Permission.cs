using ConvenienceStore.Domain.Common;

namespace ConvenienceStore.Domain.Entities.Identity;

public class Permission : BaseEntity<int>
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Module { get; private set; } = default!;

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Permission() { }
    
    public Permission(int id, string code, string name, string module) : base(id)
    {
        Code = code;
        Name = name;
        Module = module;
    }
}
