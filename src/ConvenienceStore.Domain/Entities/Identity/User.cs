using ConvenienceStore.Domain.Common;

namespace ConvenienceStore.Domain.Entities.Identity;

public class User : AuditableEntity<Guid>, ISoftDeletable
{
    public string Username { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public bool IsActive { get; private set; } = default!;

    // Quản lý Refresh Token
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTimeUtc { get; private set; }

    // Xóa mềm
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOnUtc { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation Properties
    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<UserStoreAssignment> _storeAssignment = new();
    public IReadOnlyCollection<UserStoreAssignment> StoreAssignments => _storeAssignment.AsReadOnly();

    private User() { } // Dành cho EF Core

    public static User Create(
        string username,
        string fullName,
        string email,
        string phoneNumber,
        string passwordHash
    )
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = username.Trim().ToLowerInvariant(),
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PhoneNumber = phoneNumber.Trim(),
            PasswordHash = passwordHash,
            IsActive = true
        };
    }

    public void UpdateRefreshToken(string refreshToken, DateTime expiryTimeUtc)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTimeUtc = expiryTimeUtc;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTimeUtc = null;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        RevokeRefreshToken(); // Đổi mật khẩu thì bắt buộc đăng xuất tất cả thiết bị
    }
}
