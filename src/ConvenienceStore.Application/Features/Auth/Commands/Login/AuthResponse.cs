namespace ConvenienceStore.Application.Features.Auth.Commands.Login;

public record class AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    Guid? PrimaryStoreId,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permission
);
