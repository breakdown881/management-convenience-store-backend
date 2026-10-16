using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Entities.Stores;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Application.Features.Stores.Commands.CreateStore;

public record class CreateStoreCommand(
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
    string Region
) : IRequest<Result<Guid>>;

public class CreateStoreCommandValidator : AbstractValidator<CreateStoreCommand>
{
    public CreateStoreCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã cửa hàng không được để trống.")
            .MaximumLength(20).WithMessage("Mã cửa hàng không được vượt quá 20 ký tự.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên cửa hàng không được để trống.")
            .MaximumLength(100).WithMessage("Tên cửa hàng không được vượt quá 100 ký tự.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Địa chỉ không được để trống.")
            .MaximumLength(250).WithMessage("Địa chỉ không được vượt quá 250 ký tự.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại không được để trống.")
            .MaximumLength(20).WithMessage("Số điện thoại không hợp lệ.");

        RuleFor(x => x.Region)
            .NotEmpty().WithMessage("Vùng/Khu vực không được để trống.")
            .MaximumLength(50).WithMessage("Vùng không được vượt quá 50 ký tự.");

        When(x => !x.Is247, () =>
        {
            RuleFor(x => x.OpeningTime)
                .NotNull().WithMessage("Vui lòng nhập giờ mở cửa cho cửa hàng không mở 24/7.");

            RuleFor(x => x.ClosingTime)
                .NotNull().WithMessage("Vui lòng nhập giờ đóng cửa cho cửa hàng không mở 24/7.");
        });
    }
}

public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, Result<Guid>>
{
    private readonly IAppDbContext _dbContext;

    public CreateStoreCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        var isCodeUnique = !await _dbContext.Stores
            .AnyAsync(s => s.Code == normalizedCode, cancellationToken);

        if (!isCodeUnique)
        {
            return Result.Failure<Guid>(Error.Conflict("Store.DuplicateCode", $"Mã cửa hàng '{normalizedCode}' đã tồn tại trong hệ thống."));
        }

        var store = Store.Create(
            normalizedCode,
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

        _dbContext.Stores.Add(store);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(store.Id);
    }
}
