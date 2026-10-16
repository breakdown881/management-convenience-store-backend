using ConvenienceStore.Domain.Enums;

namespace ConvenienceStore.Application.Features.Stores;

public record class StoreDto(
    Guid Id,
    string Code,
    string Name,
    string Address,
    string PhoneNumber,
    string? Email,
    bool Is247,
    TimeSpan? OpeningTime,
    TimeSpan? ClosingTime,
    double? Latitude,
    double? Longitude,
    string Region,
    StoreStatus Status
);
