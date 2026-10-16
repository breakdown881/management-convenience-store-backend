using ConvenienceStore.Domain.Entities.Audit;
using ConvenienceStore.Domain.Entities.Identity;
using ConvenienceStore.Domain.Entities.Stores;
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
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Store> Stores { get; }
    DbSet<PosTerminal> PosTerminals { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
