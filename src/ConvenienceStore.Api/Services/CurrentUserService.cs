using System.Security.Claims;
using ConvenienceStore.Application.Common.Interfaces;

namespace ConvenienceStore.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");

    public string? Username =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public Guid? StoreId =>
        Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue("store_id"), out var id)
        ? id
        : null;
}
