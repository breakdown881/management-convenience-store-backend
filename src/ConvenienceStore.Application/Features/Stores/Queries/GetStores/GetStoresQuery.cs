using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Application.Common.Models;
using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Application.Features.Stores.Queries.GetStores;

public record class GetStoresQuery(
    string? SearchTerm,
    StoreStatus? Status,
    string? Region,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PagedResult<StoreDto>>>;

public class GetStoresQueryHandler : IRequestHandler<GetStoresQuery, Result<PagedResult<StoreDto>>>
{
    private readonly IAppDbContext _dbContext;

    public GetStoresQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<StoreDto>>> Handle(GetStoresQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Stores.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            query = query.Where(s =>
                s.Code.ToLower().Contains(search) ||
                s.Name.ToLower().Contains(search) ||
                s.Address.ToLower().Contains(search));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(s => s.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Region))
        {
            query = query.Where(s => s.Region == request.Region.Trim());
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(s => s.Code)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StoreDto(
                s.Id,
                s.Code,
                s.Name,
                s.Address,
                s.PhoneNumber,
                s.Email,
                s.Is247,
                s.OpeningTime,
                s.ClosingTime,
                s.Latitude,
                s.Longitude,
                s.Region,
                s.Status
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<StoreDto>.Create(items, request.PageNumber, request.PageSize, totalCount);
        return Result.Success(pagedResult);
    }
}
