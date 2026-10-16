using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Application.Features.Stores.Queries.GetStoreById;

public record class GetStoreByIdQuery(Guid Id) : IRequest<Result<StoreDto>>;

public class GetStoreByIdQueryHandler : IRequestHandler<GetStoreByIdQuery, Result<StoreDto>>
{
    private readonly IAppDbContext _dbContext;

    public GetStoreByIdQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<StoreDto>> Handle(GetStoreByIdQuery request, CancellationToken cancellationToken)
    {
        var store = await _dbContext.Stores.AsNoTracking()
            .Where(s => s.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (store == null)
        {
            return Result.Failure<StoreDto>(Error.NotFound("Store.NotFound", $"Không tìm thấy cửa hàng với ID {request.Id}."));
        }

        return Result.Success(store);
    }
}
