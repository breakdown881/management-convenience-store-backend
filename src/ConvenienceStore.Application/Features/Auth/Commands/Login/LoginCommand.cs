using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<Result<AuthResponse>>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Tên đăng nhập không được để trống.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IAppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator
    )
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Tìm User trực tiếp qua _context.Users (gợi ý code đầy đủ, an toàn kiểu)
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .Include(u => u.StoreAssignments)
            .FirstOrDefaultAsync(u => u.Username == request.Username.Trim().ToLowerInvariant(), cancellationToken);

        if (user == null || !user.IsActive)
        {
            return Result.Failure<AuthResponse>(Error.Failure("Auth.InvalidCredentials", "Tên đăng nhập hoặc mật khẩu không chính xác."));
        }

        // 2. Xác thực mật khẩu qua BCrypt
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return Result.Failure<AuthResponse>(Error.Failure("Auth.InvalidCredentials", "Tên đăng nhập hoặc mật khẩu không chính xác."));
        }

        // 3. Lấy thông tin Roles & Permissions
        var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var primaryStore = user.StoreAssignments.FirstOrDefault(sa => sa.IsPrimary && sa.IsActive);
        var primaryStoreId = primaryStore?.StoreId;

        // 4. Sinh Token
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions, primaryStoreId);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // 5. Cập nhật RefreshToken vào Database
        user.UpdateRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse(
            accessToken,
            refreshToken,
            3600,
            new UserDto(user.Id, user.Username, user.FullName, user.Email, primaryStoreId, roles, permissions)
        );

        return Result.Success(response);
    }
}
