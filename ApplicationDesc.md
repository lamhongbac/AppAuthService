# FnB Inventory Application Description
## Mô tả tổng quát##:
- Đây là ứng dụng quản lý hàng hóa cho chuỗi nhà hàng, trong đó mỗi nhà hàng được xem như 1 kho sử dụng, bên cạnh các kho trung tâm chứa hàng
- Ứng dụng này sẽ liên kết đến ứng dụng bán hàng , qua module recipe (công thức món) để tính toán hàng đã xuất bán
- Ứng dụng này hỗ trợ quản lý chặt chẽ thông qua số hóa qui trình và tính toán dự trù (forcast) đảm bảo an ninh hàng hóa

** Đối tượng Ứng dụng**:

-CompanyId:1(Company Name: Morning Star AppTech)
-Name:FnB Inventory
-Number:FnBInv

## Đối tượng các App Object trong ứng dụng##:

** Qui định đặt mã cho đối tượng AppObject **:
- Mã đối tượng AppObject= Mã đối tượng + mã nghiệp vụ/Giao dịch
*** Qui định Mã đối tượng***

-Stock=STK chỉ tồn kho
-Supplier=SUP nhà cc
-StockLevel=STKL mức tồn kho
-Unit=UN đơn vị tính
-WareHouse=WH kho lưu trữ
-Store= kho tiêu hao hay kho sử dụng  
-Inventory=INV (inventory item)item có tính chất đếm
-Goods= hàng hóa nói chung


*** Qui định mã phân loại nghiệp vụ***
Quản trị=M (management)
Giao dịch=Tùy thuộc tên giao dịch (xem chi tiết)
Review=RV (review là duyệt)
Request=REQ(yêu cầu)

** Các hoạt động Quản trị danh mục **:

1.StockMaster: (STKM)/quản trị danh mục stock/Inventory Item
2.Suppliers: (SUPM)/quản trị danh mục nhà cung cấp
3.StockLevel: (STKLM)/quản trị các  stock level như: safe stocklevel, Min/Max Order,..etc.
4.Units: (UNM)/ Stockkeep Unit; quản trị danh mục đơn vị tính
5.WareHouses: (WHM)/quản trị danh mục kho 

** Các nghiệp vụ/Giao dịch **:

*** Các giao dich chung, phối hợp ***:
6.OpenStockItem: (INV_OP) / Lần đầu cho 1 stockItem mới xuất hiện lần đầu
7.OpenStockperiod:(STK_OP)/ Mở kỳ lần đầu, hoặc đóng kỳ khi đến hạn, sẽ xây dựng lại stock Opening và closing
8.PurchaseRequest:(PR)/Phiếu Yêu cầu mua hàng lập bởi mua hàng gửi đến phụ trách bộ phận mua hàng
9.PurchaseOrder: (PO)/Lệnh mua hàng  chuyển từ PurchaseRequest sang, và lập theo NCC và gửi đến NCC
10.InventoryRequest:(INVR) / Phiếu yêu cầu hàng hóa gửi đến kho hàng
11.InventoryRequestReview:(INV_RV) / duyệt yêu cầu  hàng hóa 
12.PurchaseRequestReview: (PO_RV) / duyệt yêu cầu mua hàng, chuyển sang lập PurchaseOrder

*** Các giao dich thay đổi stock balance ***:
13.StockIn/Goods receiving:(STK_IN) /Phiếu Nhập kho, khi ncc giao hàng
14.StockOut: (STK_OUT) /xuất kho, khi có yc hàng đc duyệt, hoặc có nghiệp vụ liên quan
15.StockOutTheory: (STK_OUT_TH)/ xuất kho do nghiệp vụ liên quan đến recipe (bán hang)
16.StockAdjust:(STK_ADJ) /điều chỉnh tồn  sau kiểm kê
17.StockTransfer IN:(STK_TFI)/ Điều chuyển đến từ kho khác
18.StockTransfer OUT:(STK_TFO)/ Điều chuyển đi đến kho khác
19.StockRETURN: (STK_RET)/ Trả hàng sau khi xuất hàng 
20.StockWASTE: (STK_WST) /Hủy hàng 

## Danh sách các App Objects đã khai báo thành công##:

| # | Mã đối tượng (Number) | Tên đối tượng (Name) | Phân loại (Type) | Mô tả (Description) | Trạng thái |
|---|:---|:---|:---|:---|:---|
| 1 | **STKM** | StockMaster | `View` | Quản trị danh mục stock | Đã khai báo |
| 2 | **SUPM** | Suppliers | `View` | Quản trị danh mục nhà cung cấp | Đã khai báo |
| 3 | **STKLM** | StockLevel | `View` | Quản trị các stock level | Đã khai báo |
| 4 | **UNM** | Units | `View` | Stockkeep Unit; quản trị danh mục đơn vị tính | Đã khai báo |
| 5 | **WHM** | WareHouses | `View` | quản trị danh mục kho | Đã khai báo |
| 6 | **INV_OP** | OpenStockItem | `Process` | Lần đầu cho 1 stockItem mới xuất hiện lần đầu | Đã khai báo |
| 7 | **STK_OP** | OpenStockperiod | `Process` | Mở kỳ lần đầu, hoặc đóng kỳ khi đến hạn | Đã khai báo |
| 8 | **PR** | PurchaseRequest | `Process` | Phiếu Yêu cầu mua hàng | Đã khai báo |
| 9 | **PO** | PurchaseOrder | `Process` | Lệnh mua hàng | Đã khai báo |
| 10 | **INVR** | InventoryRequest | `Process` | Phiếu yêu cầu hàng hóa gửi đến kho hàng | Đã khai báo |
| 11 | **INV_RV** | InventoryRequestReview | `Process` | duyệt yêu cầu hàng hóa | Đã khai báo |
| 12 | **PO_RV** | PurchaseRequestReview | `Process` | duyệt yêu cầu mua hàng | Đã khai báo |
| 13 | **STK_IN** | StockIn | `Data` | Phiếu Nhập kho, khi ncc giao hàng | Đã khai báo |
| 14 | **STK_OUT** | StockOut | `Data` | xuất kho, khi có yc hàng đc duyệt | Đã khai báo |
| 15 | **STK_OUT_TH** | StockOutTheory | `Data` | xuất kho do nghiệp vụ recipe | Đã khai báo |
| 16 | **STK_ADJ** | StockAdjust | `Data` | điều chỉnh tồn sau kiểm kê | Đã khai báo |
| 17 | **STK_TFI** | StockTransfer IN | `Data` | Điều chuyển đến từ kho khác | Đã khai báo |
| 18 | **STK_TFO** | StockTransfer OUT | `Data` | Điều chuyển đi đến kho khác | Đã khai báo |
| 19 | **STK_RET** | StockRETURN | `Data` | Trả hàng sau khi xuất hàng | Đã khai báo |
| 20 | **STK_WST** | StockWASTE | `Data` | Hủy hàng | Đã khai báo |

## Danh sách các App Roles được thiết lập mẫu##:

| # | Tên Role (Role Name) | Mã Role (Role Number) | Mô tả (Description) | Danh sách đối tượng có full quyền |
|---|:---|:---|:---|:---|
| 1 | **StockAdmin** | STK_ADMIN | Quản trị nhập các master data | STKM, UNM, SUPM, WHM |
| 2 | **StockUser** | STK_USER | Lập InventoryRequest | INVR |
| 3 | **StockManager** | STK_MGR | Duyệt InventoryRequestReview + quyền StockUser | INV_RV, INVR |
| 4 | **Buyer** | BUYER | Lập PR/PO | PR, PO |
| 5 | **PurchasingManager**| PUR_MGR | Duyệt PurchaseRequestReview + quyền Buyer | PO_RV, PR, PO |
| 6 | **StockTrans** | STK_TRANS | Thực hiện các giao dịch thay đổi stock balance | STK_IN, STK_OUT, STK_ADJ, STK_TFI, STK_TFO, STK_RET |
| 7 | **POSStock** | POS_STK | Xuất kho recipe | STK_OUT_TH |

## Danh sách các App Users được thiết lập mẫu##:

| # | Tên đầy đủ (Full Name) | Tên đăng nhập (UserName) | Mật khẩu (Password) | Vai trò (Role Mapping) |
|---|:---|:---|:---|:---|
| 1 | **Lâm Hồng Bắc** | `bac` | `Pass123!` | STK_ADMIN |
| 2 | **Nguyễn Hoài Nam** | `nam` | `Pass123!` | STK_USER |
| 3 | **Phạm Minh Châu** | `chau` | `Pass123!` | STK_MGR |
| 4 | **Lê Thu Trang** | `trang` | `Pass123!` | BUYER |
| 5 | **Trần Hoài Thu** | `thu` | `Pass123!` | PUR_MGR |
| 6 | **Ngô Thanh Tùng** | `tung` | `Pass123!` | STK_TRANS |
| 7 | **Đỗ Minh Thức** | `thuc` | `Pass123!` | POS_STK |