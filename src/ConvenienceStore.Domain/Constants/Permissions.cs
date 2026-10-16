namespace ConvenienceStore.Domain.Constants;

public static class Permissions
{
    // Phân hệ Bán hàng (POS)
    public const string PosCheckout = "Permission.Pos.Checkout";
    public const string PosVoidBill = "Permissions.Pos.VoidBill";
    public const string PosReturn = "Permissions.Pos.Return";
    public const string PosOpenDrawer = "Permissions.Pos.OpenDrawer";

    // Phân hệ Quản lý Ca (Shifts)
    public const string ShiftOpen = "Permissions.Shift.Open";
    public const string ShiftClose = "Permission.Shift.Close";
    public const string ShiftPettyCash = "Permission.Shift.PettyCash";

    // Phân hệ Kho hàng (Inventory)
    public const string InventoryView = "Permissions.Inventory.View";
    public const string InventoryAdjust = "Permission.Inventory.Adjust";
    public const string InventoryTransfer = "Permission.Inventory.Transfer";
    public const string InventoryScrap = "Permissions.Inventory.Scrap";

    // Phân hệ Hàng hóa & Bảng giá (Catalog)
    public const string CatalogView = "Permissions.Catalog.View";
    public const string CatalogManage = "Permissions.Catalog.Manage";
    public const string PriceChange = "Permissions.Catalog.PriceChange";

    // Phân hệ Cửa hàng & Thiết bị (Stores & Terminals)
    public const string StoreView = "Permissions.Store.View";
    public const string StoreManage = "Permissions.Store.Manage";
    public const string TerminalManage = "Permissions.Terminal.Manage";
}
