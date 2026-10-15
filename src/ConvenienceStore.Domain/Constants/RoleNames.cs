namespace ConvenienceStore.Domain.Constants;

/// <summary>
/// 6 Vai trò nghiệp vụ chuẩn mực trong chuỗi bán lẻ.
/// </summary>
public static class RoleNames
{
    public const string AdminDirector = "AdminDirector"; // Ban Giám Đốc / Quản trị hệ thống
    public const string StoreManager = "StoreManager"; // Cửa Hàng Trưởng
    public const string Cashier = "Cashier"; // Thu Ngân tại quầy
    public const string InventoryPurchasing = "InventoryPurchasing"; // Nhân viên Kho & Thu mua
    public const string Accountant = "Accountant"; // Kế toán kiểm soát
    public const string MarketingCRM = "MarketingCRM"; // Marketing & Chăm sóc khách hàng
}
