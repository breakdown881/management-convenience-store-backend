using ConvenienceStore.Domain.Common;

namespace ConvenienceStore.Domain.Entities.Identity;

public class UserStoreAssignment : AuditableEntity<Guid>
{
    public Guid UserId { get; private set; }
    public Guid StoreId { get; private set; }
    public bool IsPrimary { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation
    public User User { get; private set; } = default!;

    private UserStoreAssignment() { }
    
    public static UserStoreAssignment Create(Guid userId, Guid storeId, bool isPrimary = false)
    {
        return new UserStoreAssignment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StoreId = storeId,
            IsPrimary = isPrimary,
            IsActive = true
        };
    }
}
