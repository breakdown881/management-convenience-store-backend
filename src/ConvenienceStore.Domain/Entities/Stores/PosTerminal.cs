using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Enums;

namespace ConvenienceStore.Domain.Entities.Stores;

public class PosTerminal : AuditableEntity<Guid>
{
    public Guid StoreId { get; private set; }
    public string TerminalCode { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? DeviceFingerprint { get; private set; }
    public PosTerminalStatus Status { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime? LastActiveAtUtc { get; private set; }

    // Navigation
    public Store Store { get; private set; } = default!;

    private PosTerminal() { }

    public static PosTerminal Create(Guid storeId, string terminalCode, string name)
    {
        return new PosTerminal
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            TerminalCode = terminalCode.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Status = PosTerminalStatus.Active
        };
    }

    public void RegisterDevice(string fingerprint, string? ipAddress)
    {
        DeviceFingerprint = fingerprint.Trim();
        IpAddress = ipAddress;
        LastActiveAtUtc = DateTime.UtcNow;
        Status = PosTerminalStatus.Active;
    }

    public void UpdateLastActive(string? ipAddress)
    {
        IpAddress = ipAddress;
        LastActiveAtUtc = DateTime.UtcNow;
    }

    public void Lock()
    {
        Status = PosTerminalStatus.Locked;
    }
    
    public void Unlock()
    {
        Status = PosTerminalStatus.Active;
    }
}
