using Microsoft.AspNetCore.Authorization;

namespace ConvenienceStore.Api.Authorization;

/// <summary>
/// Đại diện cho yêu cầu phân quyền dựa trên mã quyền cụ thể (Permission Code)
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
