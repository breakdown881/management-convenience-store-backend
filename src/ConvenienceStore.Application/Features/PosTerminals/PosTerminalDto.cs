using ConvenienceStore.Domain.Enums;

namespace ConvenienceStore.Application.Features.PosTerminals;

public record class PosTerminalDto(
    Guid id,
    Guid StoreId,
    string TerminalCode,
    string Name,
    bool IsRegistered,
    PosTerminalStatus Status,
    string? IpAddress,
    DateTime? LastActiveAtUtc
);
