# BẢNG THEO DÕI TIẾN ĐỘ DỰ ÁN (PROJECT ROADMAP & TASK TRACKER)
> **Dự án:** Hệ thống Quản lý Chuỗi Cửa hàng Tiện lợi (Convenience Store Chain Management Backend API)  
> **Kiến trúc:** .NET 8/9 Clean Architecture, CQRS, PostgreSQL, Redis, Event-Driven  
> **Cập nhật lần cuối:** 2026-10-13

---

## 📊 TỔNG QUAN TIẾN ĐỘ THỰC HIỆN

| Giai đoạn (Phase) | Số đầu việc (Tasks) | Đã hoàn thành | Tiến độ | Trạng thái |
| :--- | :---: | :---: | :---: | :--- |
| **Giai đoạn 0: Khởi tạo Kiến trúc & Hạ tầng** | 7 | 7 | 100% | ✅ Hoàn thành |
| **Giai đoạn 1: MVP - Vận hành Cốt lõi** | 16 | 4 | 25.0% | 🔄 Đang triển khai |
| **Giai đoạn 2: Chuỗi Cung ứng & Tối ưu** | 18 | 0 | 0% | ⏳ Chưa bắt đầu |
| **Giai đoạn 3: Enterprise, Omnichannel & AI** | 15 | 0 | 0% | ⏳ Chưa bắt đầu |
| **TỔNG CỘNG TOÀN DỰ ÁN** | **56** | **11** | **19.6%** | 🔄 Đang triển khai |

---

## 📌 GIAI ĐOẠN 0: KHỞI TẠO KIẾN TRÚC & HẠ TẦNG CƠ BẢN (FOUNDATION & SETUP)
*Mục tiêu: Thiết lập Solution .NET theo Clean Architecture chuẩn enterprise, Docker Compose cho PostgreSQL/Redis và các lớp dùng chung (Shared Kernel).*

- [x] `TASK-001` Khởi tạo Solution `.sln` và các project theo Clean Architecture:
  - `ConvenienceStore.Domain` (Classlib)
  - `ConvenienceStore.Application` (Classlib)
  - `ConvenienceStore.Infrastructure` (Classlib)
  - `ConvenienceStore.Api` (Web API)
  - `ConvenienceStore.UnitTests` & `ConvenienceStore.IntegrationTests`
- [x] `TASK-002` Thiết lập `docker-compose.yml` chứa: PostgreSQL 16+, Redis 7+, pgAdmin.
- [x] `TASK-003` Xây dựng tầng Domain Shared Kernel: `BaseEntity<TId>`, `IAggregateRoot`, `ValueObject`, `DomainEvent`, `AuditableEntity`.
- [x] `TASK-004` Xây dựng Response Envelope chuẩn (`Result<T>`, `PagedResult<T>`) và `GlobalExceptionHandlerMiddleware` (chuẩn RFC 7807 ProblemDetails).
- [x] `TASK-005` Cấu hình MediatR CQRS Pipeline Behaviors: `ValidationBehavior` (FluentValidation), `LoggingBehavior` (Serilog), `PerformanceBehavior`.
- [x] `TASK-006` Cấu hình EF Core DbContext với PostgreSQL (Npgsql), Interceptor tự động gán Audit Info (`CreatedOnUtc`, `ModifiedOnUtc`).
- [x] `TASK-007` Tích hợp Swagger/OpenAPI có hỗ trợ nhập JWT Bearer Token và Header `X-Idempotency-Key`.

---

## 🛒 GIAI ĐOẠN 1: MVP - VẬN HÀNH CỐT LÕI (CORE POS & STORE OPERATIONS)
*Mục tiêu: Đưa vào hoạt động các tính năng tối thiểu để mở cửa hàng, nhập hàng, quản lý ca và bán hàng bằng máy quét barcode tại quầy.*

### 1. Phân quyền & Định danh (IAM & Security)
- [x] `TASK-101` Thiết kế Entity `User`, `Role`, `Permission`, `UserStoreAssignment`.
- [x] `TASK-102` Xây dựng API Đăng nhập, Đăng xuất, Cấp JWT Access Token & Refresh Token an toàn.
- [x] `TASK-103` Xây dựng RBAC Authorization Handler hỗ trợ 6 vai trò: `Cashier`, `StoreManager`, `InventoryPurchasing`, `MarketingCRM`, `Accountant`, `AdminDirector`.
- [x] `TASK-104` Xây dựng bảng `AuditLogs` và Middleware tự động ghi vết các thao tác nhạy cảm (Sửa giá, hủy đơn, mở két).

### 2. Quản lý Tổ chức & Điểm bán (Store & Branch)
- [ ] `TASK-105` CRUD Thông tin Cửa hàng / Chi nhánh (Mã, tên, địa chỉ, loại hình 24/7, tọa độ GPS, vùng địa lý).
- [ ] `TASK-106` Quản lý Quầy thu ngân & Máy POS: Đăng ký thiết bị (Hardware Fingerprint), gán máy POS vào chi nhánh và kho hàng mặc định.

### 3. Danh mục Hàng hóa & Bảng giá (Catalog & Pricing)
- [ ] `TASK-107` Thiết kế Entity `Product`, `Category`, `Brand`, `UnitOfMeasure`, `ProductBarcode`.
- [ ] `TASK-108` Hỗ trợ Đa mã vạch (Multi-barcode) và Giải mã mã vạch cân điện tử (Weight-embedded barcode).
- [ ] `TASK-109` Quy đổi đơn vị tính đa cấp (Base Unit: Lon $\rightarrow$ Đơn vị quy đổi: Lốc, Thùng) và tự động quy đổi số lượng tồn kho.
- [ ] `TASK-110` Thiết lập Bảng giá bán lẻ tiêu chuẩn, Thuế suất VAT (0%, 5%, 8%, 10%) và tính giá vốn ban đầu.
- [ ] `TASK-111` API xuất dữ liệu in tem mã vạch dán sản phẩm và Bảng giá cài kệ hàng (Shelf Tags).

### 4. Quản lý Kho cơ bản (Basic Inventory)
- [ ] `TASK-112` Quản lý tồn kho theo chi nhánh (`StoreInventory`): Số lượng khả dụng (On-hand), số lượng giữ chỗ (Reserved).
- [ ] `TASK-113` Ghi nhận Thẻ kho (Stock Card) chi tiết cho mọi biến động xuất/nhập kho.
- [ ] `TASK-114` Kiểm kê kho (Cycle Count / Stocktake): Nhập số đếm thực tế, lập phiếu cân bằng kho tự động và ghi nhận chênh lệch.
- [ ] `TASK-115` Xuất hủy hàng hư hỏng/hết date (Write-off / Scrap) kèm lý do và phê duyệt của Store Manager.

### 5. Vận hành POS & Quản lý Ca (Web POS & Shift Operations)
- [ ] `TASK-116` Quản lý Ca làm việc (Shift): Mở ca nhập tiền lẻ, Phiếu Thu/Chi tại quầy (Petty Cash), Rút tiền vào két an toàn (Drop Cash).
- [ ] `TASK-117` Quy trình đóng ca **Blind Drop**: Thu ngân kiểm đếm tiền thực tế theo từng mệnh giá, hệ thống so khớp tự động tính Thừa/Thiếu và in báo cáo Z-Report.
- [ ] `TASK-118` API Tra cứu Barcode tốc độ cao ($< 50\text{ms}$ qua Dapper + Redis Cache) phục vụ thao tác quét phím tắt.
- [ ] `TASK-119` Xử lý Đơn hàng: Giỏ hàng đa nhiệm (Hold bill / Recall bill), tính toán VAT, trừ tồn kho tức thời (xử lý Concurrency).
- [ ] `TASK-120` Đổi/Trả hàng (Return & Refund) và Hủy đơn (Void bill) bắt buộc xác thực mã PIN của Quản lý.

---

## 🚚 GIAI ĐOẠN 2: CHUỖI CUNG ỨNG & QUẢN TRỊ TỐI ƯU (SUPPLY CHAIN & OPTIMIZATION)
*Mục tiêu: Quản lý hàng theo Lô/Date (FEFO), điều chuyển Kho tổng, công thức pha chế món tại quầy, khuyến mãi đa dạng, tích điểm CRM và hóa đơn điện tử.*

### 1. Quản lý Lô & Hạn sử dụng (Batch & Expiry Date - FEFO)
- [ ] `TASK-201` Thiết kế Entity `ProductBatch` lưu Số lô, Ngày sản xuất (MFG) và Hạn sử dụng (EXP).
- [ ] `TASK-202` Cơ chế xuất kho và bán hàng theo nguyên tắc **FEFO (Hết hạn trước xuất trước)**.
- [ ] `TASK-203` Dashboard & Background Job cảnh báo hàng cận date trước $X$ ngày theo ngành hàng.

### 2. Chuỗi Cung ứng & Điều chuyển Nội bộ (Supply Chain & Stock Transfer)
- [ ] `TASK-204` Quản lý Yêu cầu bổ sung hàng từ Cửa hàng lên Kho tổng (Replenishment Request - PR).
- [ ] `TASK-205` Quy trình Điều chuyển kho 2 bước (Kho xuất duyệt $\rightarrow$ Trạng thái In-transit $\rightarrow$ Kho nhận quét mã đối soát thực nhận).
- [ ] `TASK-206` Điều chuyển hàng ngang giữa Cửa hàng A và Cửa hàng B.

### 3. Chế biến tại chỗ (Lightweight BOM / Recipe)
- [ ] `TASK-207` Thiết kế cấu trúc Công thức món (Recipe/BOM) cho đồ ăn nhanh/nước uống pha chế (cà phê, bánh mì, xúc xích).
- [ ] `TASK-208` Cơ chế tự động trừ kho nguyên vật liệu tương ứng ngay khi đơn hàng hoàn tất tại máy POS.

### 4. Thu mua & Nhà cung cấp (Procurement & 3-Way Matching)
- [ ] `TASK-209` Quản lý danh mục Nhà cung cấp, Hợp đồng thương mại, Lead time và Chiết khấu doanh số (Rebate).
- [ ] `TASK-210` Quản lý Đơn đặt hàng mua (PO) và Phiếu nhập kho từ NCC (GRN).
- [ ] `TASK-211` Quy trình **Đối chiếu 3 bên (3-Way Matching)** tự động kiểm tra sai lệch: $PO \leftrightarrow GRN \leftrightarrow Invoice$ trước khi kế toán duyệt thanh toán.
- [ ] `TASK-212` Nhập hàng trực tiếp tại Cửa hàng (Direct Store Delivery - DSD) cho nhà cung cấp thực phẩm tươi.

### 5. Khuyến mãi & Khách hàng thân thiết (Promotion Engine & Loyalty)
- [ ] `TASK-213` Xây dựng Promotion Rule Engine: Mua X tặng Y (BOGO), Giảm giá bậc thang theo số lượng, Combo cố định, Giảm giá dòng hàng.
- [ ] `TASK-214` Chương trình Xả hàng Giờ vàng (Happy Hour) tự động áp dụng sau 20:00 tối cho thực phẩm tươi.
- [ ] `TASK-215` Module Khách hàng thân thiết: Định danh SĐT, Tích/Tiêu điểm trực tiếp trên bill, Xếp hạng thành viên (Silver, Gold, Platinum).
- [ ] `TASK-216` Quản lý và áp dụng mã E-Voucher / Coupon giảm giá.

### 6. Thanh toán Nâng cao & Hóa đơn Điện tử (Payment & e-Invoice)
- [ ] `TASK-217` Tích hợp sinh mã **Dynamic VietQR** theo số tiền đơn hàng và Webhook nhận kết quả từ Ngân hàng / Ví điện tử (MoMo, ZaloPay, VNPay).
- [ ] `TASK-218` Tích hợp phát hành Hóa đơn điện tử (e-Invoice) khởi tạo từ máy tính tiền kết nối cơ quan Thuế.

---

## 🌐 GIAI ĐOẠN 3: ENTERPRISE, OMNICHANNEL & AI (HIGH SCALE & ENTERPRISE)
*Mục tiêu: Đạt chuẩn chuỗi bán lẻ quy mô lớn: Chế độ bán offline, tự động đặt hàng AI, bán hàng qua GrabMart/ShopeeFood, AI Camera chống thất thoát, sơ đồ kệ hàng và nhượng quyền.*

### 1. Bán hàng Ngoại tuyến (Offline-First Architecture)
- [ ] `TASK-301` Thiết kế API Đồng bộ ngoại tuyến (Offline Sync API) tiếp nhận dữ liệu batch khi máy POS có mạng trở lại.
- [ ] `TASK-302` Thuật toán xử lý xung đột dữ liệu (Conflict Resolution) cho các hóa đơn bán trong thời gian offline.

### 2. Tự động Đặt hàng & Dự báo Nhu cầu (Auto-Replenishment)
- [ ] `TASK-303` Cấu hình định mức Min/Max cho từng SKU theo từng cửa hàng.
- [ ] `TASK-304` Thuật toán tính tốc độ bán (Sale Velocity) và Lead time để tự động tạo Phiếu đề xuất đặt hàng (Suggested PO) cuối ngày.

### 3. Bán hàng Đa kênh (Omnichannel & Quick Commerce)
- [ ] `TASK-305` Đồng bộ Real-time Danh mục, Giá và Tồn kho qua API của GrabMart, ShopeeFood, BeFood, Zalo Mini App.
- [ ] `TASK-306` Webhook tiếp nhận đơn hàng online, tự động định tuyến về cửa hàng gần nhất và bắn tín hiệu chuông báo POS để nhân viên soạn hàng (Picking & Packing).

### 4. AI Camera & Chống thất thoát (Loss Prevention & Vision)
- [ ] `TASK-307` API xuất luồng dữ liệu giao dịch POS nhúng vào Video Camera quầy thu ngân (POS Video Overlay).
- [ ] `TASK-308` Công cụ lọc các giao dịch bất thường (Hủy bill, mở két không bán, hoàn tiền) để xem lại video 30 giây thời điểm đó.
- [ ] `TASK-309` Tiếp nhận dữ liệu AI Camera đếm khách (People Counting) và tính toán Tỷ lệ chuyển đổi mua hàng (Conversion Rate).
- [ ] `TASK-310` Quản lý sơ đồ kệ hàng chuẩn (Planogram Compliance) và API tiếp nhận ảnh chụp kệ hàng đối soát tuân thủ.

### 5. Quản trị Nhượng quyền (Franchise Management)
- [ ] `TASK-311` Phân tách dữ liệu tài chính & kho giữa Cửa hàng trực thuộc chuỗi và Cửa hàng nhượng quyền.
- [ ] `TASK-312` Tự động tính toán Phí bản quyền định kỳ (Royalty Fee) và Phí Marketing theo % doanh số/lợi nhuận.

### 6. Phân tích Quản trị Chuyên sâu (Advanced BI & Store P&L)
- [ ] `TASK-313` Thuật toán Phân tích Giỏ hàng (Market Basket Analysis) tìm các cặp sản phẩm mua kèm để gợi ý trưng bày/upsell.
- [ ] `TASK-314` Báo cáo Lãi/Lỗ từng cửa hàng thời gian thực (Real-time Store P&L): Doanh thu $-$ COGS $-$ Hao hụt $-$ Chi phí vận hành $\longrightarrow$ Lợi nhuận ròng (Net Profit / EBITDA).
- [ ] `TASK-315` Real-time Dashboard toàn chuỗi ứng dụng SignalR.

---

## 📝 HƯỚNG DẪN CẬP NHẬT TIẾN ĐỘ (MAINTENANCE GUIDELINES)

1. Khi bắt đầu làm một task: Đổi trạng thái từ `[ ]` sang `[ ] (Đang làm)` hoặc cập nhật nhật ký commit.
2. Khi hoàn thành và test xong một task: Đổi `[ ]` thành `[x]`, cập nhật số lượng task hoàn thành tại bảng **Tổng quan tiến độ**.
3. Mọi thay đổi về phạm vi yêu cầu nghiệp vụ đều phải được đồng bộ song song giữa [FEATURES.md](file:///D:/manage-convenience-store/FEATURES.md) và [ROADMAP.md](file:///D:/manage-convenience-store/ROADMAP.md).
