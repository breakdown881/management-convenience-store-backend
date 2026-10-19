using ConvenienceStore.Domain.Entities.Audit;
using ConvenienceStore.Domain.Entities.Catalog;
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

    // Phân hệ Danh mục Hàng hóa (Catalog)
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<UnitOfMeasure> UnitOfMeasure { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductBarcode> ProductBarcodes { get; }
    DbSet<ProductUnitConversion> ProductUnitConversions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
