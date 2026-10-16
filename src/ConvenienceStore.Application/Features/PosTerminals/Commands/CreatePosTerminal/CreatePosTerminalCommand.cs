using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Entities.Stores;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Application.Features.PosTerminals.Commands.CreatePosTerminal;

public record class CreatePosTerminalCommand(
    Guid StoreId,
    string TerminalCode,
    string Name
) : IRequest<Result<Guid>>;

public class CreatePosTerminalCommandValidator : AbstractValidator<CreatePosTerminalCommand>
{
    public CreatePosTerminalCommandValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty().WithMessage("ID cửa hàng không hợp lệ.");
        RuleFor(x => x.TerminalCode).NotEmpty().MaximumLength(20).WithMessage("Mã quầy không hợp lệ.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("Tên quầy không được để trống.");
    }
}

public class CreatePosTerminalCommandHandler : IRequestHandler<CreatePosTerminalCommand, Result<Guid>>
{
    private readonly IAppDbContext _dbContext;

    public CreatePosTerminalCommandHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreatePosTerminalCommand request, CancellationToken cancellationToken)
    {
        var storeExists = await _dbContext.Stores.AnyAsync(s => s.Id == request.StoreId, cancellationToken);
        if (!storeExists)
        {
            return Result.Failure<Guid>(Error.NotFound("Store.NotFound", $"Không tìm thấy cửa hàng với ID {request.StoreId}."));
        }

        var normalizedCode = request.TerminalCode.Trim().ToUpperInvariant();
        var isCodeUniqueInStore = !await _dbContext.PosTerminals
            .AnyAsync(pt => pt.StoreId == request.StoreId && pt.TerminalCode == normalizedCode, cancellationToken);

        if (!isCodeUniqueInStore)
        {
            return Result.Failure<Guid>(Error.Conflict("PosTerminal.DuplicateCode", $"Mã quầy '{normalizedCode}' đã tồn tại trong chi nhánh này."));
        }

        var terminal = PosTerminal.Create(request.StoreId, normalizedCode, request.Name);
        _dbContext.PosTerminals.Add(terminal);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(terminal.Id);
    }
}