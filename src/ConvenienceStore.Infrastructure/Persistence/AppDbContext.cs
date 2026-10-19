using System.Reflection;
using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Constants;
using ConvenienceStore.Domain.Entities.Audit;
using ConvenienceStore.Domain.Entities.Catalog;
using ConvenienceStore.Domain.Entities.Identity;
using ConvenienceStore.Domain.Entities.Stores;
using ConvenienceStore.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

using AppPermissions = ConvenienceStore.Domain.Constants.Permissions;

namespace ConvenienceStore.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    private readonly AuditableEntityInterceptor _auditableInterceptor;

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserStoreAssignment> UserStoreAssignments => Set<UserStoreAssignment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<PosTerminal> PosTerminals => Set<PosTerminal>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<UnitOfMeasure> UnitOfMeasure => Set<UnitOfMeasure>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductBarcode> ProductBarcodes => Set<ProductBarcode>();
    public DbSet<ProductUnitConversion> ProductUnitConversions => Set<ProductUnitConversion>();

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        AuditableEntityInterceptor auditableEntityInterceptor
    ) : base(options)
    {
        _auditableInterceptor = auditableEntityInterceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        SeedIdentityData(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private static void SeedIdentityData(ModelBuilder modelBuilder)
    {
        // 1. Seed Roles
        modelBuilder.Entity<Role>().HasData(
            new Role(1, RoleNames.AdminDirector, "Ban Giám Đốc điều hành chuỗi"),
            new Role(2, RoleNames.StoreManager, "Quản lý Cửa hàng trưởng"),
            new Role(3, RoleNames.Cashier, "Thu ngân bán hàng tại quầy"),
            new Role(4, RoleNames.InventoryPurchasing, "Quản lý Kho & Thu mua hàng hóa"),
            new Role(5, RoleNames.Accountant, "Kế toán kiểm soát doanh thu & hóa đơn"),
            new Role(6, RoleNames.MarketingCRM, "Marketing, Khuyến mãi & Loyalty")
        );

        // 2. Seed Permissions
        modelBuilder.Entity<Permission>().HasData(
            new Permission(1, AppPermissions.PosCheckout, "Thanh toán đơn hàng", "POS"),
            new Permission(2, AppPermissions.PosVoidBill, "Hủy hóa đơn", "POS"),
            new Permission(3, AppPermissions.PosReturn, "Đổi trả hàng", "POS"),
            new Permission(4, AppPermissions.PosOpenDrawer, "Mở két tiền khẩn cấp", "POS"),
            new Permission(5, AppPermissions.ShiftOpen, "Mở ca làm việc", "Shift"),
            new Permission(6, AppPermissions.ShiftClose, "Đóng ca làm việc", "Shift"),
            new Permission(7, AppPermissions.InventoryView, "Xem tồn kho", "Inventory"),
            new Permission(8, AppPermissions.CatalogManage, "Quản lý sản phẩm", "Catalog")
        );

        // 3. Gán Full quyền cho Admin (Role 1)
        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission { RoleId = 1, PermissionId = 1 },
            new RolePermission { RoleId = 1, PermissionId = 2 },
            new RolePermission { RoleId = 1, PermissionId = 3 },
            new RolePermission { RoleId = 1, PermissionId = 4 },
            new RolePermission { RoleId = 1, PermissionId = 5 },
            new RolePermission { RoleId = 1, PermissionId = 6 },
            new RolePermission { RoleId = 1, PermissionId = 7 },
            new RolePermission { RoleId = 1, PermissionId = 8 }
        );

        // 4. Gán quyền cho Thu ngân (Role 3: Checkout, ShiftOpen, ShiftClose)
        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission { RoleId = 3, PermissionId = 1 },
            new RolePermission { RoleId = 3, PermissionId = 5 },
            new RolePermission { RoleId = 3, PermissionId = 6 }
        );

        // 5. Seed Tài khoản Admin mặc định (Password: Admin@123456)
        // Hash BCrypt mẫu: $2a$12$5Jv1J0KqOqu8B0U5sPspKueN8l5iW76s7a1Cgq7uG0u9K2H4qL76C
        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        modelBuilder.Entity<User>().HasData(new
        {
            Id = adminId,
            Username = "admin",
            FullName = "Hệ Thống Quản Trị Viên",
            Email = "admin@conveniencestore.com",
            PhoneNumber = "0901234567",
            PasswordHash = "$2a$12$5Jv1J0KqOqu8B0U5sPspKueN8l5iW76s7a1Cgq7uG0u9K2H4qL76C",
            IsActive = true,
            IsDeleted = false,
            CreatedOnUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<UserRole>().HasData(new UserRole
        {
            UserId = adminId,
            RoleId = 1
        });
    }
}
