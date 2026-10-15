using ConvenienceStore.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace ConvenienceStore.Api.Authorization;

/// <summary>
/// Xử lý đánh giá quyền: Đối chiếu Claims "permission" trong JWT của người dùng
/// với yêu cầu PermissionRequirement của endpoint.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        // 1. Nếu người dùng chưa xác thực (chưa có token hoặc token không hợp lệ) -> Bỏ qua
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            return Task.CompletedTask;
        }

        // 2. SUPERADMIN BYPASS: Ban Giám Đốc (AdminDirector) có toàn quyền truy cập mọi tính năng
        if (context.User.IsInRole(RoleNames.AdminDirector))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // 3. Kiểm tra xem người dùng có sở hữu claim "permission" trùng khớp không
        var hasPermission = context.User.Claims
            .Any(c => c.Type == "permission" &&
                    string.Equals(c.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
