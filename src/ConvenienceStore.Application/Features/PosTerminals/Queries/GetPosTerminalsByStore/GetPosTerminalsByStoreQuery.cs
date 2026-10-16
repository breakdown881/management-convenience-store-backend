using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Application.Features.PosTerminals.Queries.GetPosTerminalsByStore;

public record class GetPosTerminalsByStoreQuery(Guid StoreId) : IRequest<Result<List<PosTerminalDto>>>;

public class GetPosTerminalsByStoreQueryHandler : IRequestHandler<GetPosTerminalsByStoreQuery, Result<List<PosTerminalDto>>>
{
    private readonly IAppDbContext _dbContext;

    public GetPosTerminalsByStoreQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<PosTerminalDto>>> Handle(GetPosTerminalsByStoreQuery request, CancellationToken cancellationToken)
    {
        var terminals = await _dbContext.PosTerminals.AsNoTracking()
            .Where(pt => pt.StoreId == request.StoreId)
            .OrderBy(pt => pt.TerminalCode)
            .Select(pt => new PosTerminalDto(
                pt.Id,
                pt.StoreId,
                pt.TerminalCode,
                pt.Name,
                !string.IsNullOrEmpty(pt.DeviceFingerprint),
                pt.Status,
                pt.IpAddress,
                pt.LastActiveAtUtc
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(terminals);
    }
}
