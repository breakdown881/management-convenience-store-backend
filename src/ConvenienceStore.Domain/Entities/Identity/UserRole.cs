namespace ConvenienceStore.Domain.Entities.Identity;

public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = default!;
}
