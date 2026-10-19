using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Application.Features.Catalog;

public record ShelfTagDto(
    Guid ProductId,
    string Sku,
    string ProductName,
    string PrimaryBarcode,
    string BaseUnitName,
    decimal StandardPrice,
    int VatPercent,
    decimal PriceAfterVat,
    DateTime PrintedAtUtc
);
public record GetShelfTagsQuery(Guid? CategoryId) : IRequest<Result<List<ShelfTagDto>>>;
public class GetShelfTagsQueryHandler : IRequestHandler<GetShelfTagsQuery, Result<List<ShelfTagDto>>>
{
    private readonly IAppDbContext _dbContext;

    public GetShelfTagsQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<ShelfTagDto>>> Handle(GetShelfTagsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Include(p => p.BaseUnit)
            .Include(p => p.Barcodes)
            .Where(p => p.IsActive);

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        var products = await query.OrderBy(p => p.Sku).ToListAsync(cancellationToken);

        var tags = products.Select(p =>
        {
            var primaryBarcode = p.Barcodes.FirstOrDefault(b => b.IsPrimary)?.BarcodeValue
                                    ?? p.Barcodes.FirstOrDefault()?.BarcodeValue
                                    ?? p.Sku;

            var vatPercent = (int)p.VatRate;
            var priceAfterVat = Math.Round(p.StandardPrice * (1 + vatPercent / 100m), 0);

            return new ShelfTagDto(
                p.Id,
                p.Sku,
                p.Name,
                primaryBarcode,
                p.BaseUnit.Name,
                p.StandardPrice,
                vatPercent,
                priceAfterVat,
                DateTime.UtcNow
            );
        }).ToList();

        return Result.Success(tags);
    }
}