namespace ConvenienceStore.Domain.Common;

/// <summary>
/// Lớp cơ sở trừu tượng cho mọi Entity trong Domain Model.
/// Quản lý định danh (Identity) và hàng đợi Domain Events.
/// </summary>
public abstract class BaseEntity<TId> : IEquatable<BaseEntity<TId>>
{
    public TId Id { get; protected set; } = default!;

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected BaseEntity(TId id)
    {
        Id = id;
    }

    // Constructor không tham số dành cho cơ chế Materialization của EF Core
    protected BaseEntity()
    {

    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (EqualityComparer<TId>.Default.Equals(Id, default!) || EqualityComparer<TId>.Default.Equals(other.Id, default!))
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public bool Equals(BaseEntity<TId>? other) => Equals((object?)other);

    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id!);

    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right) => Equals(left, right);

    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right) => Equals(left, right);
}