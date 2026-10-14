namespace ConvenienceStore.Domain.Common;

/// <summary>
/// Đại diện cho một sự kiện nghiệp vụ xảy ra trong Domain.
/// Bất biến (Immutable) và mang ý nghĩa trong quá khứ (Past tense).
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOnUtc { get; }
}