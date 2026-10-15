using Microsoft.AspNetCore.Authorization;

namespace ConvenienceStore.Api.Authorization;

/// <summary>
/// Attribute phân quyền declarative sử dụng trên Action hoặc Controller.
/// Ví dụ: [HasPermission(Permissions.PosCheckout)]
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(policy: permission)
    {
    }
}
