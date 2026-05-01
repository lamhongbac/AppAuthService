# Tóm tắt Session Làm việc - AppAuthService & FnB Inventory

**Thời gian thực hiện**: 01/05/2026

## 1. Mục tiêu và Kết quả đạt được
- Khai báo đầy đủ ứng dụng **FnB Inventory** và toàn bộ dữ liệu mẫu (AppObjects, Roles, Rights, Users) vào cơ sở dữ liệu `AppAuthService` từ xa.
- Tài liệu hóa toàn bộ mô tả kiến trúc và kết quả khai báo vào file [ApplicationDesc.md](ApplicationDesc.md).
- Đảm bảo 100% Unit Test của hệ thống hoạt động ổn định và vượt qua thành công.

## 2. Chi tiết 20 App Objects đã khai báo (FnB Inventory)

| # | Mã đối tượng (Number) | Tên đối tượng (Name) | Phân loại (Type) | Mô tả (Description) |
|---|:---|:---|:---|:---|
| 1 | **STKM** | StockMaster | `View` | Quản trị danh mục stock |
| 2 | **SUPM** | Suppliers | `View` | Quản trị danh mục nhà cung cấp |
| 3 | **STKLM** | StockLevel | `View` | Quản trị các stock level |
| 4 | **UNM** | Units | `View` | Stockkeep Unit; quản trị danh mục đơn vị tính |
| 5 | **WHM** | WareHouses | `View` | Quản trị danh mục kho |
| 6 | **INV_OP** | OpenStockItem | `Process` | Lần đầu cho 1 stockItem mới xuất hiện lần đầu |
| 7 | **STK_OP** | OpenStockperiod | `Process` | Mở kỳ lần đầu, hoặc đóng kỳ khi đến hạn |
| 8 | **PR** | PurchaseRequest | `Process` | Phiếu Yêu cầu mua hàng |
| 9 | **PO** | PurchaseOrder | `Process` | Lệnh mua hàng |
| 10 | **INVR** | InventoryRequest | `Process` | Phiếu yêu cầu hàng hóa gửi đến kho hàng |
| 11 | **INV_RV** | InventoryRequestReview | `Process` | Duyệt yêu cầu hàng hóa |
| 12 | **PO_RV** | PurchaseRequestReview | `Process` | Duyệt yêu cầu mua hàng |
| 13 | **STK_IN** | StockIn | `Data` | Phiếu Nhập kho, khi ncc giao hàng |
| 14 | **STK_OUT** | StockOut | `Data` | Xuất kho, khi có yc hàng đc duyệt |
| 15 | **STK_OUT_TH** | StockOutTheory | `Data` | Xuất kho do nghiệp vụ recipe |
| 16 | **STK_ADJ** | StockAdjust | `Data` | Điều chỉnh tồn sau kiểm kê |
| 17 | **STK_TFI** | StockTransfer IN | `Data` | Điều chuyển đến từ kho khác |
| 18 | **STK_TFO** | StockTransfer OUT | `Data` | Điều chuyển đi đến kho khác |
| 19 | **STK_RET** | StockRETURN | `Data` | Trả hàng sau khi xuất hàng |
| 20 | **STK_WST** | StockWASTE | `Data` | Hủy hàng |

## 3. Các App Roles đã thiết lập mẫu

| # | Tên Role (Role Name) | Mã Role (Role Number) | Mô tả (Description) | Danh sách đối tượng có full quyền |
|---|:---|:---|:---|:---|
| 1 | **StockAdmin** | STK_ADMIN | Quản trị nhập các master data | STKM, UNM, SUPM, WHM |
| 2 | **StockUser** | STK_USER | Lập InventoryRequest | INVR |
| 3 | **StockManager** | STK_MGR | Duyệt InventoryRequestReview + quyền StockUser | INV_RV, INVR |
| 4 | **Buyer** | BUYER | Lập PR/PO | PR, PO |
| 5 | **PurchasingManager**| PUR_MGR | Duyệt PurchaseRequestReview + quyền Buyer | PO_RV, PR, PO |
| 6 | **StockTrans** | STK_TRANS | Thực hiện các giao dịch thay đổi stock balance | STK_IN, STK_OUT, STK_ADJ, STK_TFI, STK_TFO, STK_RET |
| 7 | **POSStock** | POS_STK | Xuất kho recipe | STK_OUT_TH |

## 4. Các App Users đã thiết lập mẫu
- **Mật khẩu chung**: `Pass123!`

| # | Tên đầy đủ (Full Name) | Tên đăng nhập (UserName) | Vai trò (Role Mapping) |
|---|:---|:---|:---|
| 1 | **Lâm Hồng Bắc** | `bac` | `STK_ADMIN` (StockAdmin) |
| 2 | **Nguyễn Hoài Nam** | `nam` | `STK_USER` (StockUser) |
| 3 | **Phạm Minh Châu** | `chau` | `STK_MGR` (StockManager) |
| 4 | **Lê Thu Trang** | `trang` | `BUYER` (Buyer) |
| 5 | **Trần Hoài Thu** | `thu` | `PUR_MGR` (PurchasingManager) |
| 6 | **Ngô Thanh Tùng** | `tung` | `STK_TRANS` (StockTrans) |
| 7 | **Đỗ Minh Thức** | `thuc` | `POS_STK` (POSStock) |

## 5. Trạng thái Git & Test
- Đã chạy 11/11 Test case và kết quả đều **Passed**.
- Đã hoàn thành commit và push thành công tất cả các thay đổi lên branch `master` của Git repository.
