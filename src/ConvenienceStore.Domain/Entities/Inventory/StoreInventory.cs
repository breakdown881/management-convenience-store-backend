using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Entities.Catalog;
using ConvenienceStore.Domain.Entities.Stores;

namespace ConvenienceStore.Domain.Entities.Inventory;

public class StoreInventory : AuditableEntity<Guid>, IAggregateRoot
{
    public Guid StoreId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal OnHandQuantity { get; private set; }
    public decimal ReservedQuantity { get; private set; }
    public decimal AvailableQuantity => OnHandQuantity - ReservedQuantity;
    public decimal ReorderPoint { get; private set; }

    // Optimistic Concurrency Token (PostgreSQL xmin)
    public uint Version { get; private set; }

    public Store Store { get; private set; } = default!;
    public Product Product { get; private set; } = default!;

    private StoreInventory() { }

    public static StoreInventory Create(Guid storeId, Guid productId, decimal initialQty = 0, decimal reorderPoint = 10)
    {
        return new StoreInventory
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ProductId = productId,
            OnHandQuantity = initialQty,
            ReservedQuantity = 0,
            ReorderPoint = reorderPoint
        };
    }

    public (decimal Before, decimal After) AddStock(decimal quantity)
    {
        var before = OnHandQuantity;
        OnHandQuantity += quantity;
        return (before, OnHandQuantity);
    }

    public (decimal Before, decimal After) DeductStock(decimal quantity)
    {
        var before = OnHandQuantity;
        OnHandQuantity -= quantity;
        return (before, OnHandQuantity);
    }
    
    public (decimal Before, decimal After, decimal Variance) ApplyStocktakeCount(decimal countedQuantity)
    {
        var before = OnHandQuantity;
        var variance = countedQuantity - before;
        OnHandQuantity = countedQuantity;
        return (before, OnHandQuantity, variance);
    }
}
