using ConvenienceStore.Domain.Entities.Identity;

namespace ConvenienceStore.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions, Guid? primaryStoreId);
    string GenerateRefreshToken();
}
