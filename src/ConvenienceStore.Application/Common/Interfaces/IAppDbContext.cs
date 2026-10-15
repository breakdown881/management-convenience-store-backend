using ConvenienceStore.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserStoreAssignment> UserStoreAssignments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
