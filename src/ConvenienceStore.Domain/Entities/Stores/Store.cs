using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Enums;

namespace ConvenienceStore.Domain.Entities.Stores;

public class Store : AuditableEntity<Guid>, ISoftDeletable, IAggregateRoot
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public string? Email { get; private set; }
    public bool Is247 { get; private set; }
    public TimeSpan? OpeningTime { get; private set; }
    public TimeSpan? ClosingTime { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public string Region { get; private set; } = null!;
    public StoreStatus Status { get; private set; }

    // Xóa mềm (Thực thi đúng hợp đồng ISoftDeletable với public setter)
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOnUtc { get; set; }
    
    public string? DeletedBy { get; set; }

    // Navigation
    private readonly List<PosTerminal> _posTerminals = new();
    public IReadOnlyCollection<PosTerminal> PosTerminals => _posTerminals.AsReadOnly();

    private Store() { }

    public static Store Create(
        string code,
        string name,
        string address,
        string phoneNumber,
        string? email,
        bool is247,
        TimeSpan? openingTime,
        TimeSpan? closingTime,
        double? latitude,
        double? longitude,
        string region
    )
    {
        return new Store
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Address = address.Trim(),
            PhoneNumber = phoneNumber.Trim(),
            Email = email?.Trim().ToLowerInvariant(),
            Is247 = is247,
            OpeningTime = is247 ? null : openingTime,
            ClosingTime = is247 ? null : closingTime,
            Latitude = latitude,
            Longitude = longitude,
            Region = region.Trim(),
            Status = StoreStatus.Active,
            IsDeleted = false
        };
    }

    public void UpdateDetails(
        string name,
        string address,
        string phoneNumber,
        string? email,
        bool is247,
        TimeSpan? openingTime,
        TimeSpan? closingTime,
        double? latitude,
        double? longitude,
        string region
    )
    {
        Name = name.Trim();
        Address = address.Trim();
        PhoneNumber = phoneNumber.Trim();
        Email = email?.Trim().ToLowerInvariant();
        Is247 = is247;
        OpeningTime = is247 ? null : openingTime;
        ClosingTime = is247 ? null : closingTime;
        Latitude = latitude;
        Longitude = longitude;
        Region = region.Trim();
    }

    public void ChangeStatus(StoreStatus status)
    {
        Status = status;
    }
    
    public void Delete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOnUtc = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
