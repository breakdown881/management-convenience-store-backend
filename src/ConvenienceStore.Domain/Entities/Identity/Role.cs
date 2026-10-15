using ConvenienceStore.Domain.Common;

namespace ConvenienceStore.Domain.Entities.Identity;

public class Role : BaseEntity<int>
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() { }
    
    public Role(int id, string name, string description) : base(id)
    {
        Name = name;
        Description = description;
    }
}
