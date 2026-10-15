namespace ConvenienceStore.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
    Guid? StoreId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
