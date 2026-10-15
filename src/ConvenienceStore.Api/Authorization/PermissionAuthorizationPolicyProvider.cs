using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ConvenienceStore.Api.Authorization;

/// <summary>
/// Tự động sinh AuthorizationPolicy động theo mã quyền được yêu cầu,
/// giúp lập trình viên không phải khai báo hàng trăm AddPolicy() thủ công trong Program.cs.
/// </summary>
public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // 1. Kiểm tra xem policy đã được cấu hình tĩnh từ trước hay chưa
        var policy = await base.GetPolicyAsync(policyName);
        if (policy != null)
        {
            return policy;
        }

        // 2. Nếu chưa có, tự động tạo Policy mới chứa PermissionRequirement với tên policyName
        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}
