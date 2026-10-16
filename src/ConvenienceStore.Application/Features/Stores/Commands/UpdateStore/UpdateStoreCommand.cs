using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Application.Features.Stores.Commands.UpdateStore;

public record class UpdateStoreCommand(
    Guid Id,
    string Name,
    string Address,
    string PhoneNumber,
    string? Email,
    bool Is247,
    TimeSpan? OpeningTime,
    TimeSpan? ClosingTime,
    double? Latitude,
    double? Longitude,
    string Region
) : IRequest<Result>;

public class UpdateStoreCommandValidator : AbstractValidator<UpdateStoreCommand>
{
    public UpdateStoreCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("ID cửa hàng không hợp lệ.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(250);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Region).NotEmpty().MaximumLength(50);
    }
}

public class UpdateStoreCommandHandler : IRequestHandler<UpdateStoreCommand, Result>
{
    private readonly IAppDbContext _dbContext;

    public UpdateStoreCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        var store = await _dbContext.Stores
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (store == null)
        {
            return Result.Failure(Error.NotFound("Store.NotFound", $"Không tìm thấy cửa hàng với ID {request.Id}."));
        }

        store.UpdateDetails(
            request.Name,
            request.Address,
            request.PhoneNumber,
            request.Email,
            request.Is247,
            request.OpeningTime,
            request.ClosingTime,
            request.Latitude,
            request.Longitude,
            request.Region
        );

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
