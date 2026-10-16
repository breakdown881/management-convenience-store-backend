using System.Net;
using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Application.Features.PosTerminals.Commands.RegisterPosDevice;

public record class RegisterPosDeviceCommand(
    Guid TerminalId,
    string DeviceFingerprint
) : IRequest<Result>;

public class ResgisterPosDeviceCommandValidator : AbstractValidator<RegisterPosDeviceCommand>
{
    public ResgisterPosDeviceCommandValidator()
    {
        RuleFor(x => x.TerminalId).NotEmpty().WithMessage("ID máy POS không hợp lệ.");
        RuleFor(x => x.DeviceFingerprint).NotEmpty().MaximumLength(255).WithMessage("Mã nhận dạng phần cứng (Fingerprint) không được để trống.");
    }
}

public class RegisterPosDeviceCommandHandler : IRequestHandler<RegisterPosDeviceCommand, Result>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public RegisterPosDeviceCommandHandler(
        IAppDbContext dbContext,
        ICurrentUserService currentUserService,
        IAuditService auditService
    )
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result> Handle(RegisterPosDeviceCommand request, CancellationToken cancellationToken)
    {
        var terminal = await _dbContext.PosTerminals
            .FirstOrDefaultAsync(pt => pt.Id == request.TerminalId, cancellationToken);

        if (terminal == null)
        {
            return Result.Failure(Error.NotFound("PosTerminal.NotFound", $"Không tìm thấy quầy POS với ID {request.TerminalId}."));
        }

        terminal.RegisterDevice(request.DeviceFingerprint, _currentUserService.IpAddress);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Ghi nhận Audit Trail cho thao tác đăng ký phần cứng thiết bị nhạy cảm
        await _auditService.LogSensitiveActionAsync(
            action: "POS_DEVICE_REGISTERED",
            entityName: "Posterminal",
            entityId: terminal.Id.ToString(),
            oldValues: null,
            newValues: new { TerminalCode = terminal.TerminalCode, DeviceFingerprint = request.DeviceFingerprint, IPAddress = _currentUserService.IpAddress },
            cancellationToken: cancellationToken
        );

        return Result.Success();
    }
}
